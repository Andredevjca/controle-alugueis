using SistemaAlugueis.Interfaces.Repositories;
using Dapper;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.Repositories;

public class RepositorioFinanceiro(ConexaoBanco conexao) : IRepositorioFinanceiro
{
    private const string Colunas = @"
        l.id AS Id, l.casa_id AS CasaId, l.contrato_id AS ContratoId, l.inquilino_id AS InquilinoId,
        l.tipo AS Tipo, l.categoria_id AS CategoriaId, l.origem AS Origem, l.descricao AS Descricao,
        l.data_lancamento AS DataLancamento, l.vencimento AS Vencimento, l.valor AS Valor,
        l.data_pagamento AS DataPagamento, l.status AS Status, l.observacoes AS Observacoes,
        l.data_cadastro AS DataCadastro, ca.nome AS CasaNome, cat.nome AS CategoriaNome,
        i.nome_completo AS InquilinoNome";

    public async Task<(IEnumerable<LancamentoFinanceiro> Itens, int Total)> ListarAsync(
        string? tipo, string? status, int? casaId, int? categoriaId, DateTime? inicio, DateTime? fim, string? busca, int pagina, int tamanho)
    {
        using var db = conexao.Criar();
        var filtro = "WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(tipo)) filtro += " AND l.tipo = @Tipo";
        if (status == "Abertos")
        {
            filtro += " AND l.status IN (@StatusPendente, @StatusAtrasado)";
        }
        else if (!string.IsNullOrWhiteSpace(status) && status != "Todos")
        {
            filtro += " AND l.status = @Status";
        }
        if (casaId.HasValue) filtro += " AND l.casa_id = @CasaId";
        if (categoriaId.HasValue) filtro += " AND l.categoria_id = @CategoriaId";
        if (inicio.HasValue) filtro += " AND l.vencimento >= @Inicio";
        if (fim.HasValue) filtro += " AND l.vencimento <= @Fim";
        if (!string.IsNullOrWhiteSpace(busca)) filtro += " AND (l.descricao LIKE @Busca OR ca.nome LIKE @Busca)";

        var parametros = new
        {
            Tipo = tipo,
            Status = status,
            StatusPendente = StatusFinanceiro.Pendente,
            StatusAtrasado = StatusFinanceiro.Atrasado,
            CasaId = casaId,
            CategoriaId = categoriaId,
            Inicio = inicio,
            Fim = fim,
            Busca = $"%{busca}%",
            Tamanho = tamanho,
            Offset = (pagina - 1) * tamanho
        };

        var total = await db.ExecuteScalarAsync<int>($@"
            SELECT COUNT(*) FROM lancamentos_financeiros l
            LEFT JOIN casas ca ON ca.id = l.casa_id
            {filtro}", parametros);

        var itens = await db.QueryAsync<LancamentoFinanceiro>($@"
            SELECT {Colunas}
            FROM lancamentos_financeiros l
            LEFT JOIN casas ca ON ca.id = l.casa_id
            LEFT JOIN categorias_financeiras cat ON cat.id = l.categoria_id
            LEFT JOIN inquilinos i ON i.id = l.inquilino_id
            {filtro}
            ORDER BY l.vencimento DESC
            LIMIT @Tamanho OFFSET @Offset", parametros);

        return (itens, total);
    }

    public async Task<LancamentoFinanceiro?> ObterPorIdAsync(int id)
    {
        using var db = conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<LancamentoFinanceiro>($@"
            SELECT {Colunas} FROM lancamentos_financeiros l
            LEFT JOIN casas ca ON ca.id = l.casa_id
            LEFT JOIN categorias_financeiras cat ON cat.id = l.categoria_id
            LEFT JOIN inquilinos i ON i.id = l.inquilino_id
            WHERE l.id = @Id", new { Id = id });
    }

    public async Task<IEnumerable<LancamentoFinanceiro>> ListarPorCasaAsync(int casaId)
    {
        using var db = conexao.Criar();
        return await db.QueryAsync<LancamentoFinanceiro>($@"
            SELECT {Colunas} FROM lancamentos_financeiros l
            LEFT JOIN casas ca ON ca.id = l.casa_id
            LEFT JOIN categorias_financeiras cat ON cat.id = l.categoria_id
            LEFT JOIN inquilinos i ON i.id = l.inquilino_id
            WHERE l.casa_id = @CasaId
            ORDER BY l.vencimento DESC", new { CasaId = casaId });
    }

    public async Task<IEnumerable<LancamentoFinanceiro>> ListarPorInquilinoAsync(int inquilinoId)
    {
        using var db = conexao.Criar();
        return await db.QueryAsync<LancamentoFinanceiro>($@"
            SELECT {Colunas} FROM lancamentos_financeiros l
            LEFT JOIN casas ca ON ca.id = l.casa_id
            LEFT JOIN categorias_financeiras cat ON cat.id = l.categoria_id
            LEFT JOIN inquilinos i ON i.id = l.inquilino_id
            WHERE l.inquilino_id = @InquilinoId
            ORDER BY l.vencimento DESC", new { InquilinoId = inquilinoId });
    }

    public async Task<int> InserirAsync(LancamentoFinanceiro lancamento)
    {
        using var db = conexao.Criar();
        return await db.ExecuteScalarAsync<int>(@"
            INSERT INTO lancamentos_financeiros (casa_id, contrato_id, inquilino_id, tipo, categoria_id, origem, descricao,
                data_lancamento, vencimento, valor, data_pagamento, status, observacoes)
            VALUES (@CasaId, @ContratoId, @InquilinoId, @Tipo, @CategoriaId, @Origem, @Descricao,
                @DataLancamento, @Vencimento, @Valor, @DataPagamento, @Status, @Observacoes);
            SELECT LAST_INSERT_ID();", lancamento);
    }

    public async Task AtualizarAsync(LancamentoFinanceiro lancamento)
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE lancamentos_financeiros SET casa_id=@CasaId, contrato_id=@ContratoId, inquilino_id=@InquilinoId,
                tipo=@Tipo, categoria_id=@CategoriaId, origem=@Origem, descricao=@Descricao, data_lancamento=@DataLancamento,
                vencimento=@Vencimento, valor=@Valor, data_pagamento=@DataPagamento, status=@Status, observacoes=@Observacoes
            WHERE id=@Id", lancamento);
    }

    public async Task CancelarAsync(int id)
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync("UPDATE lancamentos_financeiros SET status=@Status WHERE id=@Id",
            new { Id = id, Status = StatusFinanceiro.Cancelado });
    }

    public async Task MarcarPagoAsync(int id, DateTime dataPagamento)
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync("UPDATE lancamentos_financeiros SET status=@Status, data_pagamento=@Data WHERE id=@Id",
            new { Id = id, Status = StatusFinanceiro.Pago, Data = dataPagamento });
    }

    public async Task MarcarAtrasadosAsync()
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE lancamentos_financeiros
            SET status = @Atrasado
            WHERE status = @Pendente AND vencimento < CURDATE()",
            new { Atrasado = StatusFinanceiro.Atrasado, Pendente = StatusFinanceiro.Pendente });
    }

    public async Task<bool> ExisteAluguelDoMesAsync(int? casaId, DateTime vencimento)
    {
        using var db = conexao.Criar();
        var total = await db.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM lancamentos_financeiros
            WHERE origem = @Origem
              AND tipo = @Tipo
              AND status <> @Cancelado
              AND casa_id <=> @CasaId
              AND YEAR(vencimento) = @Ano
              AND MONTH(vencimento) = @Mes",
            new
            {
                Origem = OrigemReceita.Aluguel,
                Tipo = TipoLancamento.Receita,
                Cancelado = StatusFinanceiro.Cancelado,
                CasaId = casaId,
                Ano = vencimento.Year,
                Mes = vencimento.Month
            });
        return total > 0;
    }

    public async Task<decimal> SomarAsync(string tipo, string? status, DateTime inicio, DateTime fim, string? origem = null)
    {
        using var db = conexao.Criar();
        var sql = @"SELECT COALESCE(SUM(valor),0) FROM lancamentos_financeiros
                    WHERE tipo=@Tipo AND vencimento BETWEEN @Inicio AND @Fim AND status <> @Cancelado";
        if (!string.IsNullOrWhiteSpace(status)) sql += " AND status = @Status";
        if (!string.IsNullOrWhiteSpace(origem)) sql += " AND origem = @Origem";
        return await db.ExecuteScalarAsync<decimal>(sql, new
        {
            Tipo = tipo,
            Status = status,
            Inicio = inicio,
            Fim = fim,
            Origem = origem,
            Cancelado = StatusFinanceiro.Cancelado
        });
    }

    public async Task<IEnumerable<LancamentoFinanceiro>> ListarVencimentosAsync(int dias)
    {
        using var db = conexao.Criar();
        return await db.QueryAsync<LancamentoFinanceiro>($@"
            SELECT {Colunas}
            FROM lancamentos_financeiros l
            LEFT JOIN casas ca ON ca.id = l.casa_id
            LEFT JOIN categorias_financeiras cat ON cat.id = l.categoria_id
            LEFT JOIN inquilinos i ON i.id = l.inquilino_id
            WHERE l.status IN (@Pendente, @Atrasado)
              AND l.vencimento BETWEEN DATE_SUB(CURDATE(), INTERVAL 15 DAY) AND DATE_ADD(CURDATE(), INTERVAL @Dias DAY)
            ORDER BY l.vencimento", new
        {
            Dias = dias,
            Pendente = StatusFinanceiro.Pendente,
            Atrasado = StatusFinanceiro.Atrasado
        });
    }
}
