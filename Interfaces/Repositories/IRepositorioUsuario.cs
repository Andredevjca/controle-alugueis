using SistemaAlugueis.Models;

namespace SistemaAlugueis.Interfaces.Repositories;

public interface IRepositorioUsuario
{
    Task<IEnumerable<Usuario>> ListarAsync();
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<int> ContarAsync();
    Task<int> InserirAsync(Usuario usuario);
    Task AtualizarAsync(Usuario usuario);
    Task AtualizarSenhaAsync(int id, string senhaHash);
}
