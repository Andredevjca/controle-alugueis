using SistemaAlugueis.Interfaces.Repositories;
using Dapper;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.Repositories;

public class RepositorioContrato(ConexaoBanco conexao) : IRepositorioContrato
{
    private const string Colunas = @"
        co.id AS Id, co.numero AS Numero, co.casa_id AS CasaId, co.inquilino_id AS InquilinoId,
        co.data_inicio AS DataInicio, co.data_termino AS DataTermino, co.data_fim AS DataFim,
        co.valor_aluguel AS ValorAluguel, co.dia_vencimento AS DiaVencimento, co.valor_caucao AS ValorCaucao,
        co.meses_caucao AS MesesCaucao, co.indice_reajuste AS IndiceReajuste, co.percentual_multa AS PercentualMulta,
        co.percentual_juros AS PercentualJuros, co.status AS Status, co.observacoes AS Observacoes,
        co.data_cadastro AS DataCadastro, ca.nome AS CasaNome, i.nome_completo AS InquilinoNome";

    public async Task<(IEnumerable<Contrato> Itens, int Total)> ListarAsync(string? busca, string? status, int pagina, int tamanho)
    {
        using var db = conexao.Criar();
        var filtro = "WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(busca))
        {
            filtro += " AND (co.numero LIKE @Busca OR ca.nome LIKE @Busca OR i.nome_completo LIKE @Busca)";
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filtro += " AND co.status = @Status";
        }

        var total = await db.ExecuteScalarAsync<int>($@"
            SELECT COUNT(*) FROM contratos co
            INNER JOIN casas ca ON ca.id = co.casa_id
            INNER JOIN inquilinos i ON i.id = co.inquilino_id
            {filtro}", new { Busca = $"%{busca}%", Status = status });

        var offset = (pagina - 1) * tamanho;
        var itens = await db.QueryAsync<Contrato>($@"
            SELECT {Colunas}
            FROM contratos co
            INNER JOIN casas ca ON ca.id = co.casa_id
            INNER JOIN inquilinos i ON i.id = co.inquilino_id
            {filtro}
            ORDER BY co.data_inicio DESC
            LIMIT @Tamanho OFFSET @Offset", new { Busca = $"%{busca}%", Status = status, Tamanho = tamanho, Offset = offset });
        return (itens, total);
    }

    public async Task<IEnumerable<Contrato>> ListarTodosAsync()
    {
        using var db = conexao.Criar();
        return await db.QueryAsync<Contrato>($@"
            SELECT {Colunas}
            FROM contratos co
            INNER JOIN casas ca ON ca.id = co.casa_id
            INNER JOIN inquilinos i ON i.id = co.inquilino_id
            ORDER BY co.data_inicio DESC");
    }

    public async Task<Contrato?> ObterPorIdAsync(int id)
    {
        using var db = conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<Contrato>($@"
            SELECT {Colunas} FROM contratos co
            INNER JOIN casas ca ON ca.id = co.casa_id
            INNER JOIN inquilinos i ON i.id = co.inquilino_id
            WHERE co.id = @Id", new { Id = id });
    }

    public async Task<Contrato?> ObterAtivoPorCasaAsync(int casaId)
    {
        using var db = conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<Contrato>($@"
            SELECT {Colunas} FROM contratos co
            INNER JOIN casas ca ON ca.id = co.casa_id
            INNER JOIN inquilinos i ON i.id = co.inquilino_id
            WHERE co.casa_id = @CasaId AND co.status = '{StatusContrato.Ativo}'
            LIMIT 1", new { CasaId = casaId });
    }

    public async Task<Contrato?> ObterAtivoPorInquilinoAsync(int inquilinoId)
    {
        using var db = conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<Contrato>($@"
            SELECT {Colunas} FROM contratos co
            INNER JOIN casas ca ON ca.id = co.casa_id
            INNER JOIN inquilinos i ON i.id = co.inquilino_id
            WHERE co.inquilino_id = @InquilinoId AND co.status = '{StatusContrato.Ativo}'
            LIMIT 1", new { InquilinoId = inquilinoId });
    }

    public async Task<IEnumerable<Contrato>> ListarPorCasaAsync(int casaId)
    {
        using var db = conexao.Criar();
        return await db.QueryAsync<Contrato>($@"
            SELECT {Colunas} FROM contratos co
            INNER JOIN casas ca ON ca.id = co.casa_id
            INNER JOIN inquilinos i ON i.id = co.inquilino_id
            WHERE co.casa_id = @CasaId
            ORDER BY co.data_inicio DESC", new { CasaId = casaId });
    }

    public async Task<IEnumerable<Contrato>> ListarPorInquilinoAsync(int inquilinoId)
    {
        using var db = conexao.Criar();
        return await db.QueryAsync<Contrato>($@"
            SELECT {Colunas} FROM contratos co
            INNER JOIN casas ca ON ca.id = co.casa_id
            INNER JOIN inquilinos i ON i.id = co.inquilino_id
            WHERE co.inquilino_id = @InquilinoId
            ORDER BY co.data_inicio DESC", new { InquilinoId = inquilinoId });
    }

    public async Task<IEnumerable<Contrato>> ListarProximosVencimentoAsync(int dias)
    {
        using var db = conexao.Criar();
        return await db.QueryAsync<Contrato>($@"
            SELECT {Colunas} FROM contratos co
            INNER JOIN casas ca ON ca.id = co.casa_id
            INNER JOIN inquilinos i ON i.id = co.inquilino_id
            WHERE co.status = '{StatusContrato.Ativo}'
              AND co.data_termino IS NOT NULL
              AND co.data_termino BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL @Dias DAY)
            ORDER BY co.data_termino", new { Dias = dias });
    }

    public async Task<int> InserirAsync(Contrato contrato)
    {
        using var db = conexao.Criar();
        return await db.ExecuteScalarAsync<int>(@"
            INSERT INTO contratos (numero, casa_id, inquilino_id, data_inicio, data_termino, valor_aluguel, dia_vencimento,
                valor_caucao, meses_caucao, indice_reajuste, percentual_multa, percentual_juros, status, observacoes)
            VALUES (@Numero, @CasaId, @InquilinoId, @DataInicio, @DataTermino, @ValorAluguel, @DiaVencimento,
                @ValorCaucao, @MesesCaucao, @IndiceReajuste, @PercentualMulta, @PercentualJuros, @Status, @Observacoes);
            SELECT LAST_INSERT_ID();", contrato);
    }

    public async Task AtualizarAsync(Contrato contrato)
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE contratos SET numero=@Numero, casa_id=@CasaId, inquilino_id=@InquilinoId, data_inicio=@DataInicio,
                data_termino=@DataTermino, valor_aluguel=@ValorAluguel, dia_vencimento=@DiaVencimento,
                valor_caucao=@ValorCaucao, meses_caucao=@MesesCaucao, indice_reajuste=@IndiceReajuste,
                percentual_multa=@PercentualMulta, percentual_juros=@PercentualJuros, observacoes=@Observacoes
            WHERE id=@Id", contrato);
    }

    public async Task EncerrarAsync(int id, DateTime dataFim, string status)
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE contratos SET status=@Status, data_fim=@DataFim WHERE id=@Id",
            new { Id = id, DataFim = dataFim, Status = status });
    }
}
