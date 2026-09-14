using SistemaAlugueis.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class FinanceiroController : Controller
{
    private readonly IServicoFinanceiro _servico;

    public FinanceiroController(IServicoFinanceiro servico)
    {
        _servico = servico;
    }

    public async Task<IActionResult> Index(string? tipo, string? status, int? casaId, int? categoriaId,
        DateTime? dataInicio, DateTime? dataFim, string? busca, int pagina = 1)
    {
        ViewData["Title"] = "Financeiro";
        return View(await _servico.ObterListaAsync(tipo, status, casaId, categoriaId, dataInicio, dataFim, busca, pagina));
    }

    public async Task<IActionResult> Criar(string tipo = TipoLancamento.Receita, int? casaId = null)
    {
        ViewData["Title"] = tipo == TipoLancamento.Despesa ? "Nova despesa" : "Nova receita";
        return View(await _servico.NovoFormularioAsync(tipo, casaId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(FinanceiroViewModel modelo)
    {
        ViewData["Title"] = modelo.Tipo == TipoLancamento.Despesa ? "Nova despesa" : "Nova receita";
        if (!ModelState.IsValid) return View(await _servico.PrepararFormularioAsync(modelo));
        await _servico.SalvarAsync(modelo);
        TempData["Sucesso"] = "Lançamento cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        ViewData["Title"] = "Editar lançamento";
        var item = await _servico.ObterFormularioAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, FinanceiroViewModel modelo)
    {
        ViewData["Title"] = "Editar lançamento";
        modelo.Id = id;
        if (!ModelState.IsValid) return View(await _servico.PrepararFormularioAsync(modelo));
        await _servico.SalvarAsync(modelo);
        TempData["Sucesso"] = "Lançamento atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pagar(int id, string? returnUrl)
    {
        await _servico.MarcarPagoAsync(id);
        TempData["Sucesso"] = "Pagamento registrado. O próximo vencimento de aluguel foi gerado quando aplicável.";
        return LocalRedirect(returnUrl ?? Url.Action(nameof(Index))!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _servico.CancelarAsync(id);
        TempData["Sucesso"] = "Lançamento cancelado.";
        return RedirectToAction(nameof(Index));
    }
}
