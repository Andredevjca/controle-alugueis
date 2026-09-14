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

public class ObservacoesController : Controller
{
    private readonly IServicoObservacao _servico;
    private readonly IServicoCasa _casas;
    private readonly IServicoInquilino _inquilinos;
    private readonly IServicoContrato _contratos;

    public ObservacoesController(
        IServicoObservacao servico,
        IServicoCasa casas,
        IServicoInquilino inquilinos,
        IServicoContrato contratos)
    {
        _servico = servico;
        _casas = casas;
        _inquilinos = inquilinos;
        _contratos = contratos;
    }

    public async Task<IActionResult> Index(string? tipo, int? casaId, string? busca)
    {
        ViewData["Title"] = "Observações";
        return View(new ObservacaoListaViewModel
        {
            Itens = await _servico.ListarAsync(tipo, casaId, busca),
            Tipo = tipo,
            CasaId = casaId,
            Busca = busca,
            Casas = (await _casas.ListarTodasAsync()).Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == casaId))
        });
    }

    public async Task<IActionResult> Criar(int? casaId, string? tipo)
    {
        ViewData["Title"] = "Nova observação";
        return View(await Montar(new ObservacaoViewModel { CasaId = casaId, Tipo = tipo ?? TipoObservacao.Casa }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ObservacaoViewModel modelo)
    {
        if (!ModelState.IsValid) return View(await Montar(modelo));
        var usuarioId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (int?)null;
        await _servico.SalvarAsync(modelo, usuarioId);
        TempData["Sucesso"] = "Observação registrada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        await _servico.ExcluirAsync(id);
        TempData["Sucesso"] = "Observação excluída.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ObservacaoViewModel> Montar(ObservacaoViewModel modelo)
    {
        modelo.Casas = (await _casas.ListarTodasAsync()).Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == modelo.CasaId));
        modelo.Inquilinos = (await _inquilinos.ListarTodosAsync()).Select(i => new SelectListItem(i.NomeCompleto, i.Id.ToString(), i.Id == modelo.InquilinoId));
        modelo.Contratos = (await _contratos.ListarTodosAsync()).Select(c => new SelectListItem($"{c.Numero} - {c.CasaNome}", c.Id.ToString(), c.Id == modelo.ContratoId));
        return modelo;
    }
}

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

public class UsuariosController : Controller
{
    private readonly IRepositorioUsuario _repositorio;

    public UsuariosController(
        IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Usuários";
        return View(await _repositorio.ListarAsync());
    }

    public IActionResult Criar()
    {
        ViewData["Title"] = "Novo usuário";
        return View(new UsuarioViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(UsuarioViewModel modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.Senha))
        {
            ModelState.AddModelError(nameof(modelo.Senha), "A senha é obrigatória.");
        }

        if (!ModelState.IsValid) return View(modelo);
        await _repositorio.InserirAsync(new Usuario
        {
            Nome = modelo.Nome,
            Email = modelo.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(modelo.Senha),
            Perfil = modelo.Perfil,
            Ativo = true
        });
        TempData["Sucesso"] = "Usuário cadastrado.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var usuario = await _repositorio.ObterPorIdAsync(id);
        if (usuario == null) return NotFound();
        ViewData["Title"] = "Editar usuário";
        return View(new UsuarioViewModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil,
            Ativo = usuario.Ativo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, UsuarioViewModel modelo)
    {
        modelo.Id = id;
        if (!ModelState.IsValid) return View(modelo);
        await _repositorio.AtualizarAsync(new Usuario
        {
            Id = id,
            Nome = modelo.Nome,
            Email = modelo.Email,
            Perfil = modelo.Perfil,
            Ativo = modelo.Ativo
        });
        if (!string.IsNullOrWhiteSpace(modelo.Senha))
        {
            await _repositorio.AtualizarSenhaAsync(id, BCrypt.Net.BCrypt.HashPassword(modelo.Senha));
        }

        TempData["Sucesso"] = "Usuário atualizado.";
        return RedirectToAction(nameof(Index));
    }
}

public class ConfiguracoesController : Controller
{
    private readonly IRepositorioConfiguracao _repositorio;

    public ConfiguracoesController(
        IRepositorioConfiguracao repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Configurações";
        return View(new ConfiguracaoViewModel { Itens = await _repositorio.ListarAsync() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(int id, string? valor)
    {
        await _repositorio.AtualizarAsync(id, valor);
        TempData["Sucesso"] = "Configuração atualizada.";
        return RedirectToAction(nameof(Index));
    }
}
