using SistemaAlugueis.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class ContasConsumoController : Controller
{
    private readonly IServicoContaConsumo _servico;

    public ContasConsumoController(IServicoContaConsumo servico)
    {
        _servico = servico;
    }

    public async Task<IActionResult> Index(string? tipo, int? casaId, string? status, string? busca, int pagina = 1)
    {
        ViewData["Title"] = "Contas de água e luz";
        return View(await _servico.ObterListaAsync(tipo, casaId, status, busca, pagina));
    }

    public async Task<IActionResult> Criar(string tipo = TipoConsumo.Agua, int? casaId = null)
    {
        ViewData["Title"] = tipo == TipoConsumo.Luz ? "Nova conta de luz" : "Nova conta de água";
        return View(await _servico.PrepararFormularioAsync(new ContaConsumoViewModel { Tipo = tipo, CasaId = casaId ?? 0 }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ContaConsumoViewModel modelo)
    {
        ViewData["Title"] = "Nova conta";
        if (!ModelState.IsValid) return View(await _servico.PrepararFormularioAsync(modelo));
        await _servico.SalvarAsync(modelo);
        TempData["Sucesso"] = "Conta cadastrada com sucesso.";
        if (modelo.CasaId > 0)
        {
            return RedirectToAction("Detalhes", "Casas", new { id = modelo.CasaId, aba = "consumo" });
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        ViewData["Title"] = "Editar conta";
        var item = await _servico.ObterFormularioAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ContaConsumoViewModel modelo)
    {
        modelo.Id = id;
        if (!ModelState.IsValid) return View(await _servico.PrepararFormularioAsync(modelo));
        await _servico.SalvarAsync(modelo);
        TempData["Sucesso"] = "Conta atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pagar(int id, string? returnUrl)
    {
        await _servico.MarcarPagoAsync(id);
        TempData["Sucesso"] = "Pagamento da conta registrado.";
        return LocalRedirect(returnUrl ?? Url.Action(nameof(Index))!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _servico.CancelarAsync(id);
        TempData["Sucesso"] = "Conta cancelada.";
        return RedirectToAction(nameof(Index));
    }
}
