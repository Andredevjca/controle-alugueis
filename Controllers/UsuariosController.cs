using SistemaAlugueis.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Services;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class UsuariosController : Controller
{
    private readonly IRepositorioUsuario _repositorio;

    public UsuariosController(
        IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Usuários";
        return View(await _repositorio.ListarAsync());
    }

    public IActionResult Criar()
    {
        ViewData["Title"] = "Novo usuário";
        return View(new UsuarioViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(UsuarioViewModel modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.Senha))
        {
            ModelState.AddModelError(nameof(modelo.Senha), "A senha é obrigatória.");
        }

        if (!ModelState.IsValid) return View(modelo);
        await _repositorio.InserirAsync(new Usuario
        {
            Nome = modelo.Nome,
            Email = modelo.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(modelo.Senha),
            Perfil = modelo.Perfil,
            Ativo = true
        });
        TempData["Sucesso"] = "Usuário cadastrado.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var usuario = await _repositorio.ObterPorIdAsync(id);
        if (usuario == null) return NotFound();
        ViewData["Title"] = "Editar usuário";
        return View(new UsuarioViewModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil,
            Ativo = usuario.Ativo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, UsuarioViewModel modelo)
    {
        modelo.Id = id;
        if (!ModelState.IsValid) return View(modelo);
        await _repositorio.AtualizarAsync(new Usuario
        {
            Id = id,
            Nome = modelo.Nome,
            Email = modelo.Email,
            Perfil = modelo.Perfil,
            Ativo = modelo.Ativo
        });
        if (!string.IsNullOrWhiteSpace(modelo.Senha))
        {
            await _repositorio.AtualizarSenhaAsync(id, BCrypt.Net.BCrypt.HashPassword(modelo.Senha));
        }

        TempData["Sucesso"] = "Usuário atualizado.";
        return RedirectToAction(nameof(Index));
    }
}
