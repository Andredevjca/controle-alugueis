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

public class CategoriasController : Controller
{
    private readonly IRepositorioCategoria _repositorio;

    public CategoriasController(
        IRepositorioCategoria repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Categorias";
        return View(await _repositorio.ListarAsync());
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
        await _repositorio.InserirAsync(new CategoriaFinanceira { Nome = modelo.Nome, Tipo = modelo.Tipo, Ativo = true });
        TempData["Sucesso"] = "Categoria cadastrada.";
        return RedirectToAction(nameof(Index));
    }
}
