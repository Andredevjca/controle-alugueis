using Microsoft.AspNetCore.Mvc;
using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class CategoriasController : Controller
{
    private readonly IServicoCategoria _servico;

    public CategoriasController(
        IServicoCategoria servico)
    {
        _servico = servico;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Categorias";
        return View(await _servico.ListarAsync());
    }

    public IActionResult Criar()
    {
        ViewData["Title"] = "Nova categoria";
        return View(new CategoriaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CategoriaViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        await _servico.CriarAsync(modelo);
        TempData["Sucesso"] = "Categoria cadastrada.";
        return RedirectToAction(nameof(Index));
    }
}
