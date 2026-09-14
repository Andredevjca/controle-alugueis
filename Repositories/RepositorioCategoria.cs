using Dapper;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.Repositories;

public class RepositorioCategoria : IRepositorioCategoria
{
    private readonly ConexaoBanco _conexao;

    public RepositorioCategoria(
        ConexaoBanco conexao)
    {
        _conexao = conexao;
    }

    public async Task<IEnumerable<CategoriaFinanceira>> ListarAsync(string? tipo = null)
    {
        using var db = _conexao.Criar();
        var sql = "SELECT id AS Id, nome AS Nome, tipo AS Tipo, ativo AS Ativo FROM categorias_financeiras WHERE ativo=1";
        if (!string.IsNullOrWhiteSpace(tipo)) sql += " AND tipo=@Tipo";
        sql += " ORDER BY tipo, nome";
        return await db.QueryAsync<CategoriaFinanceira>(sql, new { Tipo = tipo });
    }

    public async Task<CategoriaFinanceira?> ObterPorIdAsync(int id)
    {
        using var db = _conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<CategoriaFinanceira>(
            "SELECT id AS Id, nome AS Nome, tipo AS Tipo, ativo AS Ativo FROM categorias_financeiras WHERE id=@Id", new { Id = id });
    }

    public async Task<int> InserirAsync(CategoriaFinanceira categoria)
    {
        using var db = _conexao.Criar();
        return await db.ExecuteScalarAsync<int>(@"
            INSERT INTO categorias_financeiras (nome, tipo, ativo) VALUES (@Nome, @Tipo, @Ativo);
            SELECT LAST_INSERT_ID();", categoria);
    }

    public async Task AtualizarAsync(CategoriaFinanceira categoria)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync("UPDATE categorias_financeiras SET nome=@Nome, tipo=@Tipo, ativo=@Ativo WHERE id=@Id", categoria);
    }
}
