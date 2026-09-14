using Dapper;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.Repositories;

public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly ConexaoBanco _conexao;

    public RepositorioUsuario(ConexaoBanco conexao)
    {
        _conexao = conexao;
    }

    private const string Colunas = @"id AS Id, nome AS Nome, email AS Email, senha_hash AS SenhaHash,
        perfil AS Perfil, ativo AS Ativo, data_cadastro AS DataCadastro";

    public async Task<IEnumerable<Usuario>> ListarAsync()
    {
        using var db = _conexao.Criar();
        return await db.QueryAsync<Usuario>($"SELECT {Colunas} FROM usuarios ORDER BY nome");
    }

    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        using var db = _conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<Usuario>($"SELECT {Colunas} FROM usuarios WHERE id=@Id", new { Id = id });
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        using var db = _conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<Usuario>($"SELECT {Colunas} FROM usuarios WHERE email=@Email", new { Email = email });
    }

    public async Task<int> ContarAsync()
    {
        using var db = _conexao.Criar();
        return await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM usuarios");
    }

    public async Task<int> InserirAsync(Usuario usuario)
    {
        using var db = _conexao.Criar();
        return await db.ExecuteScalarAsync<int>(@"
            INSERT INTO usuarios (nome, email, senha_hash, perfil, ativo)
            VALUES (@Nome, @Email, @SenhaHash, @Perfil, @Ativo);
            SELECT LAST_INSERT_ID();", usuario);
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE usuarios SET nome=@Nome, email=@Email, perfil=@Perfil, ativo=@Ativo WHERE id=@Id", usuario);
    }

    public async Task AtualizarSenhaAsync(int id, string senhaHash)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync("UPDATE usuarios SET senha_hash=@SenhaHash WHERE id=@Id", new { Id = id, SenhaHash = senhaHash });
    }
}
