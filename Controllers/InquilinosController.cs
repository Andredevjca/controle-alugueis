using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Services;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class InquilinosController(ServicoInquilino servico) : Controller
{
    public async Task<IActionResult> Index(string? busca, int pagina = 1)
    {
        ViewData["Title"] = "Inquilinos";
        const int tamanho = 10;
        var (itens, total) = await servico.ListarAsync(busca, pagina, tamanho);
        return View(new InquilinoListaViewModel
        {
            Itens = itens,
            Busca = busca,
            Pagina = pagina,
            TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanho))
        });
    }

    public async Task<IActionResult> Detalhes(int id)
    {
        ViewData["Title"] = "Detalhes do inquilino";
        try
        {
            return View(await servico.ObterDetalhesAsync(id));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    public IActionResult Criar()
    {
        ViewData["Title"] = "Novo inquilino";
        return View(new InquilinoViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(InquilinoViewModel modelo)
    {
        ViewData["Title"] = "Novo inquilino";
        if (!ModelState.IsValid) return View(modelo);
        var id = await servico.SalvarAsync(modelo);
        TempData["Sucesso"] = "Inquilino cadastrado com sucesso.";
        return RedirectToAction(nameof(Detalhes), new { id });
    }

    public async Task<IActionResult> Editar(int id)
    {
        ViewData["Title"] = "Editar inquilino";
        var item = await servico.ObterAsync(id);
        return item == null ? NotFound() : View(ServicoInquilino.ParaFormulario(item));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, InquilinoViewModel modelo)
    {
        ViewData["Title"] = "Editar inquilino";
        modelo.Id = id;
        if (!ModelState.IsValid) return View(modelo);
        await servico.SalvarAsync(modelo);
        TempData["Sucesso"] = "Inquilino atualizado com sucesso.";
        return RedirectToAction(nameof(Detalhes), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        try
        {
            await servico.ExcluirAsync(id);
            TempData["Sucesso"] = "Inquilino inativado com sucesso.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Informacoes(int id)
    {
        var item = await servico.ObterAsync(id);
        if (item == null) return NotFound();
        return Json(new { item.NomeCompleto, item.Cpf, item.Telefone, item.Email, item.Situacao });
    }
}

public class ContratosController(ServicoContrato servico, ServicoCasa casas, ServicoInquilino inquilinos) : Controller
{
    public async Task<IActionResult> Index(string? busca, string? status, int pagina = 1)
    {
        ViewData["Title"] = "Contratos";
        const int tamanho = 10;
        var (itens, total) = await servico.ListarAsync(busca, status, pagina, tamanho);
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
        var contrato = await servico.ObterAsync(id);
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
            var id = await servico.SalvarAsync(modelo);
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
        var contrato = await servico.ObterAsync(id);
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
            await servico.SalvarAsync(modelo);
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
        var contrato = await servico.ObterAsync(id);
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
        await servico.EncerrarAsync(modelo.Id, modelo.DataSaida, StatusContrato.Encerrado);
        TempData["Sucesso"] = "Contrato encerrado. O histórico do inquilino foi preservado.";
        return RedirectToAction(nameof(Detalhes), new { id = modelo.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        await servico.EncerrarAsync(id, DateTime.Today, StatusContrato.Cancelado);
        TempData["Sucesso"] = "Contrato cancelado. O histórico foi preservado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ContratoViewModel> MontarFormulario(ContratoViewModel modelo)
    {
        var listaCasas = await casas.ListarTodasAsync();
        var listaInquilinos = await inquilinos.ListarTodosAsync();
        modelo.Casas = listaCasas.Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == modelo.CasaId));
        modelo.Inquilinos = listaInquilinos.Select(i => new SelectListItem(i.NomeCompleto, i.Id.ToString(), i.Id == modelo.InquilinoId));
        return modelo;
    }
}
