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

public class RelatoriosController : Controller
{
    private readonly IServicoRelatorio _servico;
    private readonly IServicoCasa _casas;
    private readonly IServicoInquilino _inquilinos;

    public RelatoriosController(
        IServicoRelatorio servico,
        IServicoCasa casas,
        IServicoInquilino inquilinos)
    {
        _servico = servico;
        _casas = casas;
        _inquilinos = inquilinos;
    }

    public async Task<IActionResult> Index(RelatorioViewModel filtro)
    {
        ViewData["Title"] = "Relatórios";
        filtro.TipoRelatorio = string.IsNullOrWhiteSpace(filtro.TipoRelatorio) ? "casas" : filtro.TipoRelatorio;
        filtro.Casas = (await _casas.ListarTodasAsync()).Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == filtro.CasaId));
        filtro.Inquilinos = (await _inquilinos.ListarTodosAsync()).Select(i => new SelectListItem(i.NomeCompleto, i.Id.ToString(), i.Id == filtro.InquilinoId));
        return View(await _servico.GerarAsync(filtro));
    }
}
