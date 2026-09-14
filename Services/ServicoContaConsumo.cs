using SistemaAlugueis.Interfaces.Services;
using System.Globalization;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoContaConsumo : IServicoContaConsumo
{
    private readonly IRepositorioContaConsumo _repositorio;

    public ServicoContaConsumo(
        IRepositorioContaConsumo repositorio)
    {
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
}
