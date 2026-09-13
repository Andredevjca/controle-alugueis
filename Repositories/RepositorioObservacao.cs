using Dapper;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.Repositories;

public interface IRepositorioContaConsumo
{
    Task<(IEnumerable<ContaConsumo> Itens, int Total)> ListarAsync(string? tipo, int? casaId, string? status, string? busca, int pagina, int tamanho);
    Task<ContaConsumo?> ObterPorIdAsync(int id);
    Task<IEnumerable<ContaConsumo>> ListarPorCasaAsync(int casaId, string? tipo = null);
    Task<int> InserirAsync(ContaConsumo conta);
    Task AtualizarAsync(ContaConsumo conta);
    Task CancelarAsync(int id);
    Task MarcarPagoAsync(int id, DateTime dataPagamento);
    Task MarcarAtrasadosAsync();
}

public class RepositorioContaConsumo(ConexaoBanco conexao) : IRepositorioContaConsumo
{
    private const string Colunas = @"
        cc.id AS Id, cc.casa_id AS CasaId, cc.tipo AS Tipo, cc.referencia AS Referencia,
        cc.leitura_anterior AS LeituraAnterior, cc.leitura_atual AS LeituraAtual, cc.consumo AS Consumo,
        cc.valor AS Valor, cc.vencimento AS Vencimento, cc.data_pagamento AS DataPagamento,
        cc.status AS Status, cc.observacoes AS Observacoes, cc.data_cadastro AS DataCadastro,
        ca.nome AS CasaNome";

    public async Task<(IEnumerable<ContaConsumo> Itens, int Total)> ListarAsync(string? tipo, int? casaId, string? status, string? busca, int pagina, int tamanho)
    {
        using var db = conexao.Criar();
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
        using var db = conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<ContaConsumo>($@"
            SELECT {Colunas} FROM contas_consumo cc
            INNER JOIN casas ca ON ca.id = cc.casa_id
            WHERE cc.id = @Id", new { Id = id });
    }

    public async Task<IEnumerable<ContaConsumo>> ListarPorCasaAsync(int casaId, string? tipo = null)
    {
        using var db = conexao.Criar();
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
        using var db = conexao.Criar();
        return await db.ExecuteScalarAsync<int>(@"
            INSERT INTO contas_consumo (casa_id, tipo, referencia, leitura_anterior, leitura_atual, consumo, valor, vencimento, data_pagamento, status, observacoes)
            VALUES (@CasaId, @Tipo, @Referencia, @LeituraAnterior, @LeituraAtual, @Consumo, @Valor, @Vencimento, @DataPagamento, @Status, @Observacoes);
            SELECT LAST_INSERT_ID();", conta);
    }

    public async Task AtualizarAsync(ContaConsumo conta)
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE contas_consumo SET casa_id=@CasaId, tipo=@Tipo, referencia=@Referencia, leitura_anterior=@LeituraAnterior,
                leitura_atual=@LeituraAtual, consumo=@Consumo, valor=@Valor, vencimento=@Vencimento,
                data_pagamento=@DataPagamento, status=@Status, observacoes=@Observacoes
            WHERE id=@Id", conta);
    }

    public async Task CancelarAsync(int id)
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync("UPDATE contas_consumo SET status=@Status WHERE id=@Id",
            new { Id = id, Status = StatusFinanceiro.Cancelado });
    }

    public async Task MarcarPagoAsync(int id, DateTime dataPagamento)
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync("UPDATE contas_consumo SET status=@Status, data_pagamento=@Data WHERE id=@Id",
            new { Id = id, Status = StatusFinanceiro.Pago, Data = dataPagamento });
    }

    public async Task MarcarAtrasadosAsync()
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE contas_consumo SET status=@Atrasado
            WHERE status=@Pendente AND vencimento < CURDATE()",
            new { Atrasado = StatusFinanceiro.Atrasado, Pendente = StatusFinanceiro.Pendente });
    }
}

public interface IRepositorioObservacao
{
    Task<IEnumerable<Observacao>> ListarAsync(string? tipo, int? casaId, string? busca);
    Task<Observacao?> ObterPorIdAsync(int id);
    Task<IEnumerable<Observacao>> ListarPorCasaAsync(int casaId);
    Task<IEnumerable<Observacao>> ListarPorInquilinoAsync(int inquilinoId);
    Task<int> InserirAsync(Observacao observacao);
    Task AtualizarAsync(Observacao observacao);
    Task ExcluirAsync(int id);
}

public class RepositorioObservacao(ConexaoBanco conexao) : IRepositorioObservacao
{
    private const string Colunas = @"
        o.id AS Id, o.titulo AS Titulo, o.descricao AS Descricao, o.tipo AS Tipo, o.casa_id AS CasaId,
        o.contrato_id AS ContratoId, o.inquilino_id AS InquilinoId, o.lancamento_id AS LancamentoId,
        o.conta_consumo_id AS ContaConsumoId, o.data_observacao AS DataObservacao, o.usuario_id AS UsuarioId,
        ca.nome AS CasaNome, i.nome_completo AS InquilinoNome, u.nome AS UsuarioNome";

    public async Task<IEnumerable<Observacao>> ListarAsync(string? tipo, int? casaId, string? busca)
    {
        using var db = conexao.Criar();
        var filtro = "WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(tipo)) filtro += " AND o.tipo = @Tipo";
        if (casaId.HasValue) filtro += " AND o.casa_id = @CasaId";
        if (!string.IsNullOrWhiteSpace(busca)) filtro += " AND (o.titulo LIKE @Busca OR o.descricao LIKE @Busca)";
        return await db.QueryAsync<Observacao>($@"
            SELECT {Colunas} FROM observacoes o
            LEFT JOIN casas ca ON ca.id = o.casa_id
            LEFT JOIN inquilinos i ON i.id = o.inquilino_id
            LEFT JOIN usuarios u ON u.id = o.usuario_id
            {filtro}
            ORDER BY o.data_observacao DESC", new { Tipo = tipo, CasaId = casaId, Busca = $"%{busca}%" });
    }

    public async Task<Observacao?> ObterPorIdAsync(int id)
    {
        using var db = conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<Observacao>($@"
            SELECT {Colunas} FROM observacoes o
            LEFT JOIN casas ca ON ca.id = o.casa_id
            LEFT JOIN inquilinos i ON i.id = o.inquilino_id
            LEFT JOIN usuarios u ON u.id = o.usuario_id
            WHERE o.id = @Id", new { Id = id });
    }

    public async Task<IEnumerable<Observacao>> ListarPorCasaAsync(int casaId)
    {
        using var db = conexao.Criar();
        return await db.QueryAsync<Observacao>($@"
            SELECT {Colunas} FROM observacoes o
            LEFT JOIN casas ca ON ca.id = o.casa_id
            LEFT JOIN inquilinos i ON i.id = o.inquilino_id
            LEFT JOIN usuarios u ON u.id = o.usuario_id
            WHERE o.casa_id = @CasaId
            ORDER BY o.data_observacao DESC", new { CasaId = casaId });
    }

    public async Task<IEnumerable<Observacao>> ListarPorInquilinoAsync(int inquilinoId)
    {
        using var db = conexao.Criar();
        return await db.QueryAsync<Observacao>($@"
            SELECT {Colunas} FROM observacoes o
            LEFT JOIN casas ca ON ca.id = o.casa_id
            LEFT JOIN inquilinos i ON i.id = o.inquilino_id
            LEFT JOIN usuarios u ON u.id = o.usuario_id
            WHERE o.inquilino_id = @InquilinoId
            ORDER BY o.data_observacao DESC", new { InquilinoId = inquilinoId });
    }

    public async Task<int> InserirAsync(Observacao observacao)
    {
        using var db = conexao.Criar();
        return await db.ExecuteScalarAsync<int>(@"
            INSERT INTO observacoes (titulo, descricao, tipo, casa_id, contrato_id, inquilino_id, lancamento_id, conta_consumo_id, data_observacao, usuario_id)
            VALUES (@Titulo, @Descricao, @Tipo, @CasaId, @ContratoId, @InquilinoId, @LancamentoId, @ContaConsumoId, @DataObservacao, @UsuarioId);
            SELECT LAST_INSERT_ID();", observacao);
    }

    public async Task AtualizarAsync(Observacao observacao)
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE observacoes SET titulo=@Titulo, descricao=@Descricao, tipo=@Tipo, casa_id=@CasaId,
                contrato_id=@ContratoId, inquilino_id=@InquilinoId
            WHERE id=@Id", observacao);
    }

    public async Task ExcluirAsync(int id)
    {
        using var db = conexao.Criar();
        await db.ExecuteAsync("DELETE FROM observacoes WHERE id=@Id", new { Id = id });
    }
}
