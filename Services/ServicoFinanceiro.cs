using SistemaAlugueis.Interfaces.Services;
using System.Globalization;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoFinanceiro : IServicoFinanceiro
{
    private readonly IRepositorioFinanceiro _financeiro;
    private readonly IRepositorioCategoria _categorias;
    private readonly IRepositorioContrato _contratos;

    public ServicoFinanceiro(IRepositorioFinanceiro financeiro, IRepositorioCategoria categorias, IRepositorioContrato contratos)
    {
        _financeiro = financeiro;
        _categorias = categorias;
        _contratos = contratos;
    }

    public Task<(IEnumerable<LancamentoFinanceiro> Itens, int Total)> ListarAsync(
        string? tipo, string? status, int? casaId, int? categoriaId, DateTime? inicio, DateTime? fim, string? busca, int pagina, int tamanho)
        => _financeiro.ListarAsync(tipo, status, casaId, categoriaId, inicio, fim, busca, pagina, tamanho);

    public Task<LancamentoFinanceiro?> ObterAsync(int id) => _financeiro.ObterPorIdAsync(id);

    public Task<IEnumerable<CategoriaFinanceira>> ListarCategoriasAsync(string? tipo = null) => _categorias.ListarAsync(tipo);

    public async Task<int> SalvarAsync(FinanceiroViewModel modelo)
    {
        var anterior = modelo.Id == 0 ? null : await _financeiro.ObterPorIdAsync(modelo.Id);
        if (modelo.Status == StatusFinanceiro.Pago && modelo.DataPagamento == null)
        {
            modelo.DataPagamento = DateTime.Today;
        }

        var entidade = Mapear(modelo);
        if (modelo.Id == 0)
        {
            var id = await _financeiro.InserirAsync(entidade);
            if (entidade.Status == StatusFinanceiro.Pago)
            {
                var criado = await _financeiro.ObterPorIdAsync(id);
                if (criado != null)
                {
                    await GerarProximoAluguelAsync(criado);
                }
            }

            return id;
        }

        await _financeiro.AtualizarAsync(entidade);
        if (entidade.Status == StatusFinanceiro.Pago && anterior?.Status != StatusFinanceiro.Pago)
        {
            var atual = await _financeiro.ObterPorIdAsync(modelo.Id);
            if (atual != null)
            {
                await GerarProximoAluguelAsync(atual);
            }
        }

        return modelo.Id;
    }

    public Task CancelarAsync(int id) => _financeiro.CancelarAsync(id);

    public async Task MarcarPagoAsync(int id)
    {
        var lancamento = await _financeiro.ObterPorIdAsync(id);
        if (lancamento == null)
        {
            return;
        }

        await _financeiro.MarcarPagoAsync(id, DateTime.Today);
        await GerarProximoAluguelAsync(lancamento);
    }

    private async Task GerarProximoAluguelAsync(LancamentoFinanceiro pago)
    {
        if (pago.Tipo != TipoLancamento.Receita || pago.Origem != OrigemReceita.Aluguel)
        {
            return;
        }

        if (pago.ContratoId.HasValue)
        {
            var contrato = await _contratos.ObterPorIdAsync(pago.ContratoId.Value);
            if (contrato == null || contrato.Status != StatusContrato.Ativo)
            {
                return;
            }
        }

        var baseVencimento = pago.Vencimento == default ? DateTime.Today : pago.Vencimento;
        var proximoMes = baseVencimento.AddMonths(1);
        var dia = Math.Min(baseVencimento.Day, DateTime.DaysInMonth(proximoMes.Year, proximoMes.Month));
        var proximoVencimento = new DateTime(proximoMes.Year, proximoMes.Month, dia);

        if (await _financeiro.ExisteAluguelDoMesAsync(pago.CasaId, proximoVencimento))
        {
            return;
        }

        var casaNome = string.IsNullOrWhiteSpace(pago.CasaNome) ? "casa" : pago.CasaNome;
        await _financeiro.InserirAsync(new LancamentoFinanceiro
        {
            CasaId = pago.CasaId,
            ContratoId = pago.ContratoId,
            InquilinoId = pago.InquilinoId,
            Tipo = TipoLancamento.Receita,
            CategoriaId = pago.CategoriaId,
            Origem = OrigemReceita.Aluguel,
            Descricao = $"Aluguel {casaNome} - {MesAno(proximoVencimento)}",
            DataLancamento = new DateTime(proximoVencimento.Year, proximoVencimento.Month, 1),
            Vencimento = proximoVencimento,
            Valor = pago.Valor,
            Status = StatusFinanceiro.Pendente
        });
    }

    private static string MesAno(DateTime data)
    {
        var cultura = new CultureInfo("pt-BR");
        var mes = data.ToString("MMMM", cultura);
        mes = cultura.TextInfo.ToTitleCase(mes);
        return $"{mes}/{data:yyyy}";
    }

    public static FinanceiroViewModel ParaFormulario(LancamentoFinanceiro l) => new()
    {
        Id = l.Id,
        CasaId = l.CasaId,
        ContratoId = l.ContratoId,
        InquilinoId = l.InquilinoId,
        Tipo = l.Tipo,
        CategoriaId = l.CategoriaId,
        Origem = l.Origem,
        Descricao = l.Descricao,
        DataLancamento = l.DataLancamento,
        Vencimento = l.Vencimento,
        Valor = l.Valor,
        DataPagamento = l.DataPagamento,
        Status = l.Status,
        Observacoes = l.Observacoes
    };

    private static LancamentoFinanceiro Mapear(FinanceiroViewModel m) => new()
    {
        Id = m.Id,
        CasaId = m.CasaId,
        ContratoId = m.ContratoId,
        InquilinoId = m.InquilinoId,
        Tipo = m.Tipo,
        CategoriaId = m.CategoriaId,
        Origem = m.Origem,
        Descricao = m.Descricao,
        DataLancamento = m.DataLancamento,
        Vencimento = m.Vencimento,
        Valor = m.Valor,
        DataPagamento = m.DataPagamento,
        Status = m.Status,
        Observacoes = m.Observacoes
    };
}
