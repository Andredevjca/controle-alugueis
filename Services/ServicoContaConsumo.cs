using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoContaConsumo : IServicoContaConsumo
{
    private readonly IRepositorioContaConsumo _repositorio;

    private readonly IRepositorioCasa _casas;

    public ServicoContaConsumo(
        IRepositorioContaConsumo repositorio, IRepositorioCasa casas)
    {
        _casas = casas;
        _repositorio = repositorio;
    }

    public Task<(IEnumerable<ContaConsumo> Itens, int Total)> ListarAsync(string? tipo, int? casaId, string? status, string? busca, int pagina, int tamanho)
        => _repositorio.ListarAsync(tipo, casaId, status, busca, pagina, tamanho);

    public Task<ContaConsumo?> ObterAsync(int id) => _repositorio.ObterPorIdAsync(id);

    public async Task<int> SalvarAsync(ContaConsumoViewModel modelo)
    {
        if (modelo.LeituraAnterior.HasValue && modelo.LeituraAtual.HasValue)
        {
            modelo.Consumo = modelo.LeituraAtual.Value - modelo.LeituraAnterior.Value;
        }

        if (modelo.Status == StatusFinanceiro.Pago && modelo.DataPagamento == null)
        {
            modelo.DataPagamento = DateTime.Today;
        }

        var entidade = Mapear(modelo);
        if (modelo.Id == 0)
        {
            return await _repositorio.InserirAsync(entidade);
        }

        await _repositorio.AtualizarAsync(entidade);
        return modelo.Id;
    }

    public Task CancelarAsync(int id) => _repositorio.CancelarAsync(id);

    public Task MarcarPagoAsync(int id) => _repositorio.MarcarPagoAsync(id, DateTime.Today);

    public static ContaConsumoViewModel ParaFormulario(ContaConsumo c) => new()
    {
        Id = c.Id,
        CasaId = c.CasaId,
        Tipo = c.Tipo,
        Referencia = c.Referencia,
        LeituraAnterior = c.LeituraAnterior,
        LeituraAtual = c.LeituraAtual,
        Consumo = c.Consumo,
        Valor = c.Valor,
        Vencimento = c.Vencimento,
        DataPagamento = c.DataPagamento,
        Status = c.Status,
        Observacoes = c.Observacoes
    };

    private static ContaConsumo Mapear(ContaConsumoViewModel m) => new()
    {
        Id = m.Id,
        CasaId = m.CasaId,
        Tipo = m.Tipo,
        Referencia = m.Referencia,
        LeituraAnterior = m.LeituraAnterior,
        LeituraAtual = m.LeituraAtual,
        Consumo = m.Consumo,
        Valor = m.Valor,
        Vencimento = m.Vencimento,
        DataPagamento = m.DataPagamento,
        Status = m.Status,
        Observacoes = m.Observacoes
    };

    public async Task<ContaConsumoListaViewModel> ObterListaAsync(string? tipo, int? casaId, string? status, string? busca, int pagina = 1)
    {
        const int tamanho = 15;

        var (itens, total) = await ListarAsync(tipo, casaId, status, busca, pagina, tamanho);

        return new ContaConsumoListaViewModel
        {
            Itens = itens,
            Tipo = tipo,
            CasaId = casaId,
            Status = status,
            Busca = busca,
            Pagina = pagina,
            TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanho)),
            Casas = (await _casas.ListarTodasAsync()).Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == casaId))
        };
    }

    public async Task<ContaConsumoViewModel> PrepararFormularioAsync(ContaConsumoViewModel modelo)
    {

        modelo.Casas = (await _casas.ListarTodasAsync()).Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == modelo.CasaId));
        return modelo;

    }

    public async Task<ContaConsumoViewModel?> ObterFormularioAsync(int id)
    {
        var entidade = await ObterAsync(id);
        if (entidade == null) return null;
        return await PrepararFormularioAsync(ParaFormulario(entidade));
    }
}
