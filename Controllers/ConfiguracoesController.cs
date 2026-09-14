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

public class ConfiguracoesController : Controller
{
    private readonly IRepositorioConfiguracao _repositorio;

    public ConfiguracoesController(IRepositorioConfiguracao repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Configurações";
        return View(new ConfiguracaoViewModel { Itens = await _repositorio.ListarAsync() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(int id, string? valor)
    {
        await _repositorio.AtualizarAsync(id, valor);
        TempData["Sucesso"] = "Configuração atualizada.";
        return RedirectToAction(nameof(Index));
    }
}
