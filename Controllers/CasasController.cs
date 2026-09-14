using SistemaAlugueis.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using SistemaAlugueis.Services;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class CasasController(IServicoCasa servico) : Controller
{
    public async Task<IActionResult> Index(string? busca, string? status, int pagina = 1)
    {
        ViewData["Title"] = "Casas";
        const int tamanho = 10;
        var (itens, total) = await servico.ListarAsync(busca, status, pagina, tamanho);
        return View(new CasaListaViewModel
        {
            Busca = busca,
            Status = status,
            Pagina = pagina,
            TotalRegistros = total,
            TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanho)),
            Itens = itens.Select(c => new CasaListaItemViewModel
            {
                Id = c.Id,
                Nome = c.Nome,
                EnderecoCompleto = $"{c.Endereco}, {c.Numero} - {c.Bairro}, {c.Cidade}/{c.Estado}",
                ValorAluguel = c.ValorAluguel,
                InquilinoAtual = c.InquilinoAtual,
                Status = c.Status
            })
        });
    }

    public async Task<IActionResult> Detalhes(int id, string aba = "resumo")
    {
        ViewData["Title"] = "Detalhes da casa";
        try
        {
            return View(await servico.ObterDetalhesAsync(id, aba));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    public IActionResult Criar()
    {
        ViewData["Title"] = "Nova casa";
        return View(new CasaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CasaViewModel modelo)
    {
        ViewData["Title"] = "Nova casa";
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            var id = await servico.SalvarAsync(modelo);
            TempData["Sucesso"] = "Casa cadastrada com sucesso.";
            return RedirectToAction(nameof(Detalhes), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    public async Task<IActionResult> Editar(int id)
    {
        ViewData["Title"] = "Editar casa";
        var casa = await servico.ObterAsync(id);
        if (casa == null)
        {
            return NotFound();
        }

        return View(ServicoCasa.ParaFormulario(casa));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, CasaViewModel modelo)
    {
        ViewData["Title"] = "Editar casa";
        modelo.Id = id;
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            await servico.SalvarAsync(modelo);
            TempData["Sucesso"] = "Casa atualizada com sucesso.";
            return RedirectToAction(nameof(Detalhes), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        try
        {
            await servico.ExcluirAsync(id);
            TempData["Sucesso"] = "Casa inativada com sucesso.";
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
        var casa = await servico.ObterAsync(id);
        if (casa == null)
        {
            return NotFound();
        }

        return Json(new
        {
            casa.Nome,
            endereco = $"{casa.Endereco}, {casa.Numero} - {casa.Bairro}",
            valor = casa.ValorAluguel,
            diaVencimento = casa.DiaVencimento,
            status = casa.Status
        });
    }
}
