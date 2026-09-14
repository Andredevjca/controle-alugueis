using Dapper;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.Repositories;

public class RepositorioContaConsumo : IRepositorioContaConsumo
{
    private readonly ConexaoBanco _conexao;

    public RepositorioContaConsumo(ConexaoBanco conexao)
    {
        _conexao = conexao;
    }

    private const string Colunas = @"
        cc.id AS Id, cc.casa_id AS CasaId, cc.tipo AS Tipo, cc.referencia AS Referencia,
        cc.leitura_anterior AS LeituraAnterior, cc.leitura_atual AS LeituraAtual, cc.consumo AS Consumo,
        cc.valor AS Valor, cc.vencimento AS Vencimento, cc.data_pagamento AS DataPagamento,
        cc.status AS Status, cc.observacoes AS Observacoes, cc.data_cadastro AS DataCadastro,
        ca.nome AS CasaNome";

    public async Task<(IEnumerable<ContaConsumo> Itens, int Total)> ListarAsync(string? tipo, int? casaId, string? status, string? busca, int pagina, int tamanho)
    {
        using var db = _conexao.Criar();
        var filtro = "WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(tipo)) filtro += " AND cc.tipo = @Tipo";
        if (casaId.HasValue) filtro += " AND cc.casa_id = @CasaId";
        if (!string.IsNullOrWhiteSpace(status)) filtro += " AND cc.status = @Status";
        if (!string.IsNullOrWhiteSpace(busca)) filtro += " AND (ca.nome LIKE @Busca OR cc.referencia LIKE @Busca)";

        var parametros = new { Tipo = tipo, CasaId = casaId, Status = status, Busca = $"%{busca}%", Tamanho = tamanho, Offset = (pagina - 1) * tamanho };
        var total = await db.ExecuteScalarAsync<int>($@"
            SELECT COUNT(*) FROM contas_consumo cc
            INNER JOIN casas ca ON ca.id = cc.casa_id
            {filtro}", parametros);
        var itens = await db.QueryAsync<ContaConsumo>($@"
            SELECT {Colunas} FROM contas_consumo cc
            INNER JOIN casas ca ON ca.id = cc.casa_id
            {filtro}
            ORDER BY cc.vencimento DESC
            LIMIT @Tamanho OFFSET @Offset", parametros);
        return (itens, total);
    }

    public async Task<ContaConsumo?> ObterPorIdAsync(int id)
    {
        using var db = _conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<ContaConsumo>($@"
            SELECT {Colunas} FROM contas_consumo cc
            INNER JOIN casas ca ON ca.id = cc.casa_id
            WHERE cc.id = @Id", new { Id = id });
    }

    public async Task<IEnumerable<ContaConsumo>> ListarPorCasaAsync(int casaId, string? tipo = null)
    {
        using var db = _conexao.Criar();
        var filtro = "WHERE cc.casa_id = @CasaId";
        if (!string.IsNullOrWhiteSpace(tipo)) filtro += " AND cc.tipo = @Tipo";
        return await db.QueryAsync<ContaConsumo>($@"
            SELECT {Colunas} FROM contas_consumo cc
            INNER JOIN casas ca ON ca.id = cc.casa_id
            {filtro}
            ORDER BY cc.referencia DESC", new { CasaId = casaId, Tipo = tipo });
    }

    public async Task<int> InserirAsync(ContaConsumo conta)
    {
        using var db = _conexao.Criar();
        return await db.ExecuteScalarAsync<int>(@"
            INSERT INTO contas_consumo (casa_id, tipo, referencia, leitura_anterior, leitura_atual, consumo, valor, vencimento, data_pagamento, status, observacoes)
            VALUES (@CasaId, @Tipo, @Referencia, @LeituraAnterior, @LeituraAtual, @Consumo, @Valor, @Vencimento, @DataPagamento, @Status, @Observacoes);
            SELECT LAST_INSERT_ID();", conta);
    }

    public async Task AtualizarAsync(ContaConsumo conta)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE contas_consumo SET casa_id=@CasaId, tipo=@Tipo, referencia=@Referencia, leitura_anterior=@LeituraAnterior,
                leitura_atual=@LeituraAtual, consumo=@Consumo, valor=@Valor, vencimento=@Vencimento,
                data_pagamento=@DataPagamento, status=@Status, observacoes=@Observacoes
            WHERE id=@Id", conta);
    }

    public async Task CancelarAsync(int id)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync("UPDATE contas_consumo SET status=@Status WHERE id=@Id",
            new { Id = id, Status = StatusFinanceiro.Cancelado });
    }

    public async Task MarcarPagoAsync(int id, DateTime dataPagamento)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync("UPDATE contas_consumo SET status=@Status, data_pagamento=@Data WHERE id=@Id",
            new { Id = id, Status = StatusFinanceiro.Pago, Data = dataPagamento });
    }

    public async Task MarcarAtrasadosAsync()
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE contas_consumo SET status=@Atrasado
            WHERE status=@Pendente AND vencimento < CURDATE()",
            new { Atrasado = StatusFinanceiro.Atrasado, Pendente = StatusFinanceiro.Pendente });
    }
}
