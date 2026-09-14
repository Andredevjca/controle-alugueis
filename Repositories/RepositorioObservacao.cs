using Dapper;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.Repositories;

public class RepositorioObservacao : IRepositorioObservacao
{
    private readonly ConexaoBanco _conexao;

    public RepositorioObservacao(
        ConexaoBanco conexao)
    {
        _conexao = conexao;
    }

    private const string Colunas = @"
        o.id AS Id, o.titulo AS Titulo, o.descricao AS Descricao, o.tipo AS Tipo, o.casa_id AS CasaId,
        o.contrato_id AS ContratoId, o.inquilino_id AS InquilinoId, o.lancamento_id AS LancamentoId,
        o.conta_consumo_id AS ContaConsumoId, o.data_observacao AS DataObservacao, o.usuario_id AS UsuarioId,
        ca.nome AS CasaNome, i.nome_completo AS InquilinoNome, u.nome AS UsuarioNome";

    public async Task<IEnumerable<Observacao>> ListarAsync(string? tipo, int? casaId, string? busca)
    {
        using var db = _conexao.Criar();
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
        using var db = _conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<Observacao>($@"
            SELECT {Colunas} FROM observacoes o
            LEFT JOIN casas ca ON ca.id = o.casa_id
            LEFT JOIN inquilinos i ON i.id = o.inquilino_id
            LEFT JOIN usuarios u ON u.id = o.usuario_id
            WHERE o.id = @Id", new { Id = id });
    }

    public async Task<IEnumerable<Observacao>> ListarPorCasaAsync(int casaId)
    {
        using var db = _conexao.Criar();
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
        using var db = _conexao.Criar();
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
        using var db = _conexao.Criar();
        return await db.ExecuteScalarAsync<int>(@"
            INSERT INTO observacoes (titulo, descricao, tipo, casa_id, contrato_id, inquilino_id, lancamento_id, conta_consumo_id, data_observacao, usuario_id)
            VALUES (@Titulo, @Descricao, @Tipo, @CasaId, @ContratoId, @InquilinoId, @LancamentoId, @ContaConsumoId, @DataObservacao, @UsuarioId);
            SELECT LAST_INSERT_ID();", observacao);
    }

    public async Task AtualizarAsync(Observacao observacao)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE observacoes SET titulo=@Titulo, descricao=@Descricao, tipo=@Tipo, casa_id=@CasaId,
                contrato_id=@ContratoId, inquilino_id=@InquilinoId
            WHERE id=@Id", observacao);
    }

    public async Task ExcluirAsync(int id)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync("DELETE FROM observacoes WHERE id=@Id", new { Id = id });
    }
}
