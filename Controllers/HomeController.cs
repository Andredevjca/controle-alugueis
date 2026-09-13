using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAlugueis.Services;

namespace SistemaAlugueis.Controllers;

public class HomeController(ServicoDashboard dashboard) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";
        try
        {
            return View(await dashboard.ObterAsync());
        }
        catch (Exception)
        {
            ViewBag.ErroBanco = "Não foi possível conectar ao MySQL. Verifique a string de conexão em appsettings.json e execute Database/schema.sql.";
            return View(new ViewModels.DashboardViewModel());
        }
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new Models.ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
