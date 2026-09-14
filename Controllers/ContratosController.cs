using SistemaAlugueis.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Services;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class ContratosController : Controller
{
    private readonly IServicoContrato _servico;
    private readonly IServicoCasa _casas;
    private readonly IServicoInquilino _inquilinos;

    public ContratosController(
        IServicoContrato servico,
        IServicoCasa casas,
        IServicoInquilino inquilinos)
    {
        _servico = servico;
        _casas = casas;
        _inquilinos = inquilinos;
    }

    public async Task<IActionResult> Index(string? busca, string? status, int pagina = 1)
    {
        ViewData["Title"] = "Contratos";
        const int tamanho = 10;
        var (itens, total) = await _servico.ListarAsync(busca, status, pagina, tamanho);
        return View(new ContratoListaViewModel
        {
            Itens = itens,
            Busca = busca,
            Status = status,
            Pagina = pagina,
            TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanho))
        });
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
        var modelo = await MontarFormulario(new ContratoViewModel
        {
            CasaId = casaId ?? 0,
            InquilinoId = inquilinoId ?? 0,
            Numero = $"{DateTime.Today:yyyy}/{DateTime.Today.Month:00}"
        });
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ContratoViewModel modelo)
    {
        ViewData["Title"] = "Novo contrato";
        if (!ModelState.IsValid)
        {
            return View(await MontarFormulario(modelo));
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
            return View(await MontarFormulario(modelo));
        }
    }

    public async Task<IActionResult> Editar(int id)
    {
        ViewData["Title"] = "Editar contrato";
        var contrato = await _servico.ObterAsync(id);
        if (contrato == null) return NotFound();
        return View(await MontarFormulario(ServicoContrato.ParaFormulario(contrato)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ContratoViewModel modelo)
    {
        ViewData["Title"] = "Editar contrato";
        modelo.Id = id;
        if (!ModelState.IsValid) return View(await MontarFormulario(modelo));
        try
        {
            await _servico.SalvarAsync(modelo);
            TempData["Sucesso"] = "Contrato atualizado com sucesso.";
            return RedirectToAction(nameof(Detalhes), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await MontarFormulario(modelo));
        }
    }

    public async Task<IActionResult> Encerrar(int id)
    {
        ViewData["Title"] = "Encerrar contrato";
        var contrato = await _servico.ObterAsync(id);
        if (contrato == null) return NotFound();
        return View(new EncerrarContratoViewModel
        {
            Id = contrato.Id,
            Numero = contrato.Numero,
            CasaNome = contrato.CasaNome ?? "",
            InquilinoNome = contrato.InquilinoNome ?? "",
            DataSaida = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Encerrar(EncerrarContratoViewModel modelo)
    {
        await _servico.EncerrarAsync(modelo.Id, modelo.DataSaida, StatusContrato.Encerrado);
        TempData["Sucesso"] = "Contrato encerrado. O histórico do inquilino foi preservado.";
        return RedirectToAction(nameof(Detalhes), new { id = modelo.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _servico.EncerrarAsync(id, DateTime.Today, StatusContrato.Cancelado);
        TempData["Sucesso"] = "Contrato cancelado. O histórico foi preservado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ContratoViewModel> MontarFormulario(ContratoViewModel modelo)
    {
        var listaCasas = await _casas.ListarTodasAsync();
        var listaInquilinos = await _inquilinos.ListarTodosAsync();
        modelo.Casas = listaCasas.Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == modelo.CasaId));
        modelo.Inquilinos = listaInquilinos.Select(i => new SelectListItem(i.NomeCompleto, i.Id.ToString(), i.Id == modelo.InquilinoId));
        return modelo;
    }
}
