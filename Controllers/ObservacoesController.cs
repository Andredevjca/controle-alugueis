using SistemaAlugueis.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class ObservacoesController : Controller
{
    private readonly IServicoObservacao _servico;

    public ObservacoesController(IServicoObservacao servico)
    {
        _servico = servico;
    }

    public async Task<IActionResult> Index(string? tipo, int? casaId, string? busca)
    {
        ViewData["Title"] = "Observações";
        return View(await _servico.ObterListaAsync(tipo, casaId, busca));
    }

    public async Task<IActionResult> Criar(int? casaId, string? tipo)
    {
        ViewData["Title"] = "Nova observação";
        return View(await _servico.PrepararFormularioAsync(new ObservacaoViewModel { CasaId = casaId, Tipo = tipo ?? TipoObservacao.Casa }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ObservacaoViewModel modelo)
    {
        if (!ModelState.IsValid) return View(await _servico.PrepararFormularioAsync(modelo));
        var usuarioId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (int?)null;
        await _servico.SalvarAsync(modelo, usuarioId);
        TempData["Sucesso"] = "Observação registrada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        await _servico.ExcluirAsync(id);
        TempData["Sucesso"] = "Observação excluída.";
        return RedirectToAction(nameof(Index));
    }
}
