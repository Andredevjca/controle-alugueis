using SistemaAlugueis.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class UsuariosController : Controller
{
    private readonly IServicoUsuario _servico;

    public UsuariosController(
        IServicoUsuario servico)
    {
        _servico = servico;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Usuários";
        return View(await _servico.ListarAsync());
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
        if (!ModelState.IsValid) return View(modelo);
        try
        {
            await _servico.CriarAsync(modelo);
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            ModelState.AddModelError(nameof(modelo.Senha), ex.Message);
            return View(modelo);
        }
        TempData["Sucesso"] = "Usuário cadastrado.";
return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var modelo = await _servico.ObterFormularioAsync(id);
        if (modelo == null) return NotFound();
        ViewData["Title"] = "Editar usuário";
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, UsuarioViewModel modelo)
    {
        modelo.Id = id;
        if (!ModelState.IsValid) return View(modelo);
        await _servico.AtualizarAsync(modelo);
        TempData["Sucesso"] = "Usuário atualizado.";
        return RedirectToAction(nameof(Index));
    }
}
