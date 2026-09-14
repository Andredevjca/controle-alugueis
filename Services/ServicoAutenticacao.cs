using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoAutenticacao : IServicoAutenticacao
{
    private readonly IRepositorioUsuario _usuarios;

    public ServicoAutenticacao(
        IRepositorioUsuario usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task GarantirAdministradorAsync()
    {
        try
        {
            if (await _usuarios.ContarAsync() > 0)
            {
                return;
            }

            await _usuarios.InserirAsync(new Models.Usuario
            {
                Nome = "Administrador",
                Email = "admin@sistema.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Perfil = PerfilUsuario.Administrador,
                Ativo = true
            });
        }
        catch
        {
            // Banco ainda não disponível na inicialização.
        }
    }

    public async Task<Models.Usuario?> ValidarAsync(string email, string senha)
    {
        var usuario = await _usuarios.ObterPorEmailAsync(email);
        if (usuario == null || !usuario.Ativo)
        {
            return null;
        }

        return BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash) ? usuario : null;
    }
}
