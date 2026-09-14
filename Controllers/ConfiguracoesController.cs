using Microsoft.AspNetCore.Mvc;
using SistemaAlugueis.Interfaces.Services;

namespace SistemaAlugueis.Controllers;

public class ConfiguracoesController : Controller
{
    private readonly IServicoConfiguracao _servico;

    public ConfiguracoesController(IServicoConfiguracao servico)
    {
        _servico = servico;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Configurações";
        return View(await _servico.ObterAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(int id, string? valor)
    {
        await _servico.AtualizarAsync(id, valor);
        TempData["Sucesso"] = "Configuração atualizada.";
        return RedirectToAction(nameof(Index));
    }
}
