using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoUsuario
{
    Task<IEnumerable<Usuario>> ListarAsync();
    Task<int> CriarAsync(UsuarioViewModel modelo);
    Task<UsuarioViewModel?> ObterFormularioAsync(int id);
    Task AtualizarAsync(UsuarioViewModel modelo);
}
