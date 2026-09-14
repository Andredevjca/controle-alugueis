using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoUsuario : IServicoUsuario
{
    private readonly IRepositorioUsuario _repositorio;

    public ServicoUsuario(IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }
    public Task<IEnumerable<Usuario>> ListarAsync() => _repositorio.ListarAsync();

    public async Task<int> CriarAsync(UsuarioViewModel modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.Senha))
            throw new System.ComponentModel.DataAnnotations.ValidationException("A senha é obrigatória.");
        return await _repositorio.InserirAsync(new Usuario
        {
            Nome = modelo.Nome, Email = modelo.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(modelo.Senha),
            Perfil = modelo.Perfil, Ativo = true
        });
    }

    public async Task<UsuarioViewModel?> ObterFormularioAsync(int id)
    {
        var usuario = await _repositorio.ObterPorIdAsync(id);
        return usuario == null ? null : new UsuarioViewModel
        {
            Id = usuario.Id, Nome = usuario.Nome, Email = usuario.Email,
            Perfil = usuario.Perfil, Ativo = usuario.Ativo
        };
    }

    public async Task AtualizarAsync(UsuarioViewModel modelo)
    {
        await _repositorio.AtualizarAsync(new Usuario
        {
            Id = modelo.Id, Nome = modelo.Nome, Email = modelo.Email,
            Perfil = modelo.Perfil, Ativo = modelo.Ativo
        });
        if (!string.IsNullOrWhiteSpace(modelo.Senha))
            await _repositorio.AtualizarSenhaAsync(modelo.Id, BCrypt.Net.BCrypt.HashPassword(modelo.Senha));
    }
}
