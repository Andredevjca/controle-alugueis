using Microsoft.AspNetCore.Mvc;
using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Services;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class InquilinosController : Controller
{
    private readonly IServicoInquilino _servico;

    public InquilinosController(IServicoInquilino servico)
    {
        _servico = servico;
    }

    public async Task<IActionResult> Index(string? busca, int pagina = 1)
    {
        ViewData["Title"] = "Inquilinos";
        const int tamanho = 10;
        var (itens, total) = await _servico.ListarAsync(busca, pagina, tamanho);
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
            return View(await _servico.ObterDetalhesAsync(id));
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
        var id = await _servico.SalvarAsync(modelo);
        TempData["Sucesso"] = "Inquilino cadastrado com sucesso.";
        return RedirectToAction(nameof(Detalhes), new { id });
    }

    public async Task<IActionResult> Editar(int id)
    {
        ViewData["Title"] = "Editar inquilino";
        var item = await _servico.ObterAsync(id);
        return item == null ? NotFound() : View(ServicoInquilino.ParaFormulario(item));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, InquilinoViewModel modelo)
    {
        ViewData["Title"] = "Editar inquilino";
        modelo.Id = id;
        if (!ModelState.IsValid) return View(modelo);
        await _servico.SalvarAsync(modelo);
        TempData["Sucesso"] = "Inquilino atualizado com sucesso.";
        return RedirectToAction(nameof(Detalhes), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        try
        {
            await _servico.ExcluirAsync(id);
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
        var item = await _servico.ObterAsync(id);
        if (item == null) return NotFound();
        return Json(new { item.NomeCompleto, item.Cpf, item.Telefone, item.Email, item.Situacao });
    }
}
