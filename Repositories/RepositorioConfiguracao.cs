using SistemaAlugueis.Interfaces.Repositories;
using Dapper;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.Repositories;

public class RepositorioConfiguracao : IRepositorioConfiguracao
{
    private readonly ConexaoBanco _conexao;

    public RepositorioConfiguracao(
        ConexaoBanco conexao)
    {
        _conexao = conexao;
    }

    public async Task<IEnumerable<Configuracao>> ListarAsync()
    {
        using var db = _conexao.Criar();
        return await db.QueryAsync<Configuracao>(
            "SELECT id AS Id, chave AS Chave, valor AS Valor, descricao AS Descricao FROM configuracoes ORDER BY id");
    }

    public async Task AtualizarAsync(int id, string? valor)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync("UPDATE configuracoes SET valor=@Valor WHERE id=@Id", new { Id = id, Valor = valor });
    }
}
