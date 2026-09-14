using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoAutenticacao
{
    Task GarantirAdministradorAsync();
    Task<Models.Usuario?> ValidarAsync(string email, string senha);
}
