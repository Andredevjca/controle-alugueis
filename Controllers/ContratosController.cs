using Microsoft.AspNetCore.Mvc;
using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class ContratosController : Controller
{
    private readonly IServicoContrato _servico;

    public ContratosController(IServicoContrato servico)
    {
        _servico = servico;
    }

    public async Task<IActionResult> Index(string? busca, string? status, int pagina = 1)
    {
        ViewData["Title"] = "Contratos";
        return View(await _servico.ObterListaAsync(busca, status, pagina));
    }

    public async Task<IActionResult> Detalhes(int id)
    {
        ViewData["Title"] = "Detalhes do contrato";
        var contrato = await _servico.ObterAsync(id);
        return contrato == null ? NotFound() : View(contrato);
    }

    public async Task<IActionResult> Criar(int? casaId, int? inquilinoId)
    {
        ViewData["Title"] = "Novo contrato";
        var modelo = await _servico.NovoFormularioAsync(casaId, inquilinoId);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ContratoViewModel modelo)
    {
        ViewData["Title"] = "Novo contrato";
        if (!ModelState.IsValid)
        {
            return View(await _servico.PrepararFormularioAsync(modelo));
        }

        try
        {
            var id = await _servico.SalvarAsync(modelo);
            TempData["Sucesso"] = "Contrato cadastrado com sucesso.";
            return RedirectToAction(nameof(Detalhes), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await _servico.PrepararFormularioAsync(modelo));
        }
    }

    public async Task<IActionResult> Editar(int id)
    {
        ViewData["Title"] = "Editar contrato";
        var contrato = await _servico.ObterFormularioAsync(id);
        if (contrato == null) return NotFound();
        return View(contrato);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ContratoViewModel modelo)
    {
        ViewData["Title"] = "Editar contrato";
        modelo.Id = id;
        if (!ModelState.IsValid) return View(await _servico.PrepararFormularioAsync(modelo));
        try
        {
            await _servico.SalvarAsync(modelo);
            TempData["Sucesso"] = "Contrato atualizado com sucesso.";
            return RedirectToAction(nameof(Detalhes), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await _servico.PrepararFormularioAsync(modelo));
        }
    }

    public async Task<IActionResult> Encerrar(int id)
    {
        ViewData["Title"] = "Encerrar contrato";
        var modelo = await _servico.ObterEncerramentoAsync(id);
        return modelo == null ? NotFound() : View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Encerrar(EncerrarContratoViewModel modelo)
    {
        await _servico.EncerrarAsync(modelo);
        TempData["Sucesso"] = "Contrato encerrado. O histórico do inquilino foi preservado.";
        return RedirectToAction(nameof(Detalhes), new { id = modelo.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _servico.CancelarAsync(id);
        TempData["Sucesso"] = "Contrato cancelado. O histórico foi preservado.";
        return RedirectToAction(nameof(Index));
    }
}
