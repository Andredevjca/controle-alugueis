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

public class ObservacoesController : Controller
{
    private readonly IServicoObservacao _servico;
    private readonly IServicoCasa _casas;
    private readonly IServicoInquilino _inquilinos;
    private readonly IServicoContrato _contratos;

    public ObservacoesController(IServicoObservacao servico, IServicoCasa casas, IServicoInquilino inquilinos, IServicoContrato contratos)
    {
        _servico = servico;
        _casas = casas;
        _inquilinos = inquilinos;
        _contratos = contratos;
    }

    public async Task<IActionResult> Index(string? tipo, int? casaId, string? busca)
    {
        ViewData["Title"] = "Observações";
        return View(new ObservacaoListaViewModel
        {
            Itens = await _servico.ListarAsync(tipo, casaId, busca),
            Tipo = tipo,
            CasaId = casaId,
            Busca = busca,
            Casas = (await _casas.ListarTodasAsync()).Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == casaId))
        });
    }

    public async Task<IActionResult> Criar(int? casaId, string? tipo)
    {
        ViewData["Title"] = "Nova observação";
        return View(await Montar(new ObservacaoViewModel { CasaId = casaId, Tipo = tipo ?? TipoObservacao.Casa }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ObservacaoViewModel modelo)
    {
        if (!ModelState.IsValid) return View(await Montar(modelo));
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

    private async Task<ObservacaoViewModel> Montar(ObservacaoViewModel modelo)
    {
        modelo.Casas = (await _casas.ListarTodasAsync()).Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == modelo.CasaId));
        modelo.Inquilinos = (await _inquilinos.ListarTodosAsync()).Select(i => new SelectListItem(i.NomeCompleto, i.Id.ToString(), i.Id == modelo.InquilinoId));
        modelo.Contratos = (await _contratos.ListarTodosAsync()).Select(c => new SelectListItem($"{c.Numero} - {c.CasaNome}", c.Id.ToString(), c.Id == modelo.ContratoId));
        return modelo;
    }
}
