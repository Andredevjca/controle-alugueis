using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoObservacao : IServicoObservacao
{
    private readonly IRepositorioObservacao _repositorio;

    private readonly IRepositorioCasa _casas;

    private readonly IRepositorioInquilino _inquilinos;

    private readonly IRepositorioContrato _contratos;

    public ServicoObservacao(
        IRepositorioObservacao repositorio, IRepositorioCasa casas, IRepositorioInquilino inquilinos, IRepositorioContrato contratos)
    {
        _contratos = contratos;
        _inquilinos = inquilinos;
        _casas = casas;
        _repositorio = repositorio;
    }

    public Task<IEnumerable<Observacao>> ListarAsync(string? tipo, int? casaId, string? busca)
        => _repositorio.ListarAsync(tipo, casaId, busca);

    public Task<Observacao?> ObterAsync(int id) => _repositorio.ObterPorIdAsync(id);

    public async Task<int> SalvarAsync(ObservacaoViewModel modelo, int? usuarioId)
    {
        var entidade = new Observacao
        {
            Id = modelo.Id,
            Titulo = modelo.Titulo,
            Descricao = modelo.Descricao,
            Tipo = modelo.Tipo,
            CasaId = modelo.CasaId,
            ContratoId = modelo.ContratoId,
            InquilinoId = modelo.InquilinoId,
            DataObservacao = DateTime.Now,
            UsuarioId = usuarioId
        };

        if (modelo.Id == 0)
        {
            return await _repositorio.InserirAsync(entidade);
        }

        await _repositorio.AtualizarAsync(entidade);
        return modelo.Id;
    }

    public Task ExcluirAsync(int id) => _repositorio.ExcluirAsync(id);

    public async Task<ObservacaoListaViewModel> ObterListaAsync(string? tipo, int? casaId, string? busca)
    {
        return new ObservacaoListaViewModel
        {
            Itens = await ListarAsync(tipo, casaId, busca),
            Tipo = tipo,
            CasaId = casaId,
            Busca = busca,
            Casas = (await _casas.ListarTodasAsync()).Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == casaId))
        };
    }

    public async Task<ObservacaoViewModel> PrepararFormularioAsync(ObservacaoViewModel modelo)
    {

        modelo.Casas = (await _casas.ListarTodasAsync()).Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == modelo.CasaId));
        modelo.Inquilinos = (await _inquilinos.ListarTodosAsync()).Select(i => new SelectListItem(i.NomeCompleto, i.Id.ToString(), i.Id == modelo.InquilinoId));
        modelo.Contratos = (await _contratos.ListarTodosAsync()).Select(c => new SelectListItem($"{c.Numero} - {c.CasaNome}", c.Id.ToString(), c.Id == modelo.ContratoId));
        return modelo;

    }
}
