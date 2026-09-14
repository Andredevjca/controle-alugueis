using SistemaAlugueis.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Controllers;

public class RelatoriosController : Controller
{
    private readonly IServicoRelatorio _servico;

    public RelatoriosController(IServicoRelatorio servico)
    {
        _servico = servico;
    }

    public async Task<IActionResult> Index(RelatorioViewModel filtro)
    {
        ViewData["Title"] = "Relatórios";
        return View(await _servico.GerarAsync(filtro));
    }
}
