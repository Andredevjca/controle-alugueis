using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoContrato : IServicoContrato
{
    private readonly IRepositorioContrato _repositorio;
    private readonly IRepositorioCasa _casas;
    private readonly IRepositorioInquilino _inquilinos;

    public ServicoContrato(
        IRepositorioContrato repositorio,
        IRepositorioCasa casas, IRepositorioInquilino inquilinos)
    {
        _inquilinos = inquilinos;
        _repositorio = repositorio;
        _casas = casas;
    }

    public Task<(IEnumerable<Contrato> Itens, int Total)> ListarAsync(string? busca, string? status, int pagina, int tamanho)
        => _repositorio.ListarAsync(busca, status, pagina, tamanho);

    public Task<IEnumerable<Contrato>> ListarTodosAsync() => _repositorio.ListarTodosAsync();

    public Task<Contrato?> ObterAsync(int id) => _repositorio.ObterPorIdAsync(id);

    public async Task<int> SalvarAsync(ContratoViewModel modelo)
    {
        var casaAtiva = await _repositorio.ObterAtivoPorCasaAsync(modelo.CasaId);
        if (casaAtiva != null && casaAtiva.Id != modelo.Id)
        {
            throw new InvalidOperationException("Esta casa já possui um contrato ativo.");
        }

        var inquilinoAtivo = await _repositorio.ObterAtivoPorInquilinoAsync(modelo.InquilinoId);
        if (inquilinoAtivo != null && inquilinoAtivo.Id != modelo.Id)
        {
            throw new InvalidOperationException("Este inquilino já possui um contrato ativo.");
        }

        var entidade = Mapear(modelo);
        entidade.Status = StatusContrato.Ativo;

        if (modelo.Id == 0)
        {
            var id = await _repositorio.InserirAsync(entidade);
            await _casas.AtualizarStatusAsync(modelo.CasaId, StatusCasa.Alugada);
            return id;
        }

        await _repositorio.AtualizarAsync(entidade);
        return modelo.Id;
    }

    public async Task EncerrarAsync(int id, DateTime dataSaida, string status)
    {
        var contrato = await _repositorio.ObterPorIdAsync(id) ?? throw new InvalidOperationException("Contrato não encontrado.");
        await _repositorio.EncerrarAsync(id, dataSaida, status);
        var outroAtivo = await _repositorio.ObterAtivoPorCasaAsync(contrato.CasaId);
        if (outroAtivo == null)
        {
            var casa = await _casas.ObterPorIdAsync(contrato.CasaId);
            if (casa != null && casa.Status != StatusCasa.Manutencao)
            {
                await _casas.AtualizarStatusAsync(contrato.CasaId, StatusCasa.Disponivel);
            }
        }
    }

    public static ContratoViewModel ParaFormulario(Contrato c) => new()
    {
        Id = c.Id,
        CasaId = c.CasaId,
        InquilinoId = c.InquilinoId,
        Numero = c.Numero,
        DataInicio = c.DataInicio,
        DataTermino = c.DataTermino,
        ValorAluguel = c.ValorAluguel,
        DiaVencimento = c.DiaVencimento,
        ValorCaucao = c.ValorCaucao,
        MesesCaucao = c.MesesCaucao,
        IndiceReajuste = c.IndiceReajuste,
        PercentualMulta = c.PercentualMulta,
        PercentualJuros = c.PercentualJuros,
        Observacoes = c.Observacoes
    };

    private static Contrato Mapear(ContratoViewModel m) => new()
    {
        Id = m.Id,
        CasaId = m.CasaId,
        InquilinoId = m.InquilinoId,
        Numero = m.Numero,
        DataInicio = m.DataInicio,
        DataTermino = m.DataTermino,
        ValorAluguel = m.ValorAluguel,
        DiaVencimento = m.DiaVencimento,
        ValorCaucao = m.ValorCaucao,
        MesesCaucao = m.MesesCaucao,
        IndiceReajuste = m.IndiceReajuste,
        PercentualMulta = m.PercentualMulta,
        PercentualJuros = m.PercentualJuros,
        Observacoes = m.Observacoes,
        Status = StatusContrato.Ativo
    };

    public async Task<ContratoListaViewModel> ObterListaAsync(string? busca, string? status, int pagina = 1)
    {
        const int tamanho = 10;

        var (itens, total) = await ListarAsync(busca, status, pagina, tamanho);

        return new ContratoListaViewModel
        {
            Itens = itens,
            Busca = busca,
            Status = status,
            Pagina = pagina,
            TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanho))
        };
    }

    public async Task<ContratoViewModel> PrepararFormularioAsync(ContratoViewModel modelo)
    {

        var listaCasas = await _casas.ListarTodasAsync();
        var listaInquilinos = await _inquilinos.ListarTodosAsync();
        modelo.Casas = listaCasas.Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == modelo.CasaId));
        modelo.Inquilinos = listaInquilinos.Select(i => new SelectListItem(i.NomeCompleto, i.Id.ToString(), i.Id == modelo.InquilinoId));
        return modelo;

    }

    public async Task<ContratoViewModel?> ObterFormularioAsync(int id)
    {
        var entidade = await ObterAsync(id);
        if (entidade == null) return null;
        return await PrepararFormularioAsync(ParaFormulario(entidade));
    }

    public async Task<ContratoViewModel> NovoFormularioAsync(int? casaId, int? inquilinoId)
    {
        return await PrepararFormularioAsync(new ContratoViewModel { CasaId = casaId ?? 0, InquilinoId = inquilinoId ?? 0, Numero = $"{DateTime.Today:yyyy}/{DateTime.Today.Month:00}" });
    }

    public async Task<EncerrarContratoViewModel?> ObterEncerramentoAsync(int id)
    {
var contrato = await ObterAsync(id);
if (contrato == null) return null;
return new EncerrarContratoViewModel
        {
            Id = contrato.Id,
            Numero = contrato.Numero,
            CasaNome = contrato.CasaNome ?? "",
            InquilinoNome = contrato.InquilinoNome ?? "",
            DataSaida = DateTime.Today
        };
    }

    public Task CancelarAsync(int id)
    {
        return EncerrarAsync(id, DateTime.Today, StatusContrato.Cancelado);
    }

    public Task EncerrarAsync(EncerrarContratoViewModel modelo)
    {
        return EncerrarAsync(modelo.Id, modelo.DataSaida, StatusContrato.Encerrado);
    }
}
