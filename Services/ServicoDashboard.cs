using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoDashboard : IServicoDashboard
{
    private readonly IRepositorioCasa _casas;
    private readonly IRepositorioContrato _contratos;
    private readonly IRepositorioFinanceiro _financeiro;
    private readonly IRepositorioContaConsumo _consumo;

    public ServicoDashboard(IRepositorioCasa casas, IRepositorioContrato contratos, IRepositorioFinanceiro financeiro, IRepositorioContaConsumo consumo)
    {
        _casas = casas;
        _contratos = contratos;
        _financeiro = financeiro;
        _consumo = consumo;
    }

    public async Task<DashboardViewModel> ObterAsync()
    {
        await _financeiro.MarcarAtrasadosAsync();
        await _consumo.MarcarAtrasadosAsync();

        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);
        var listaCasas = (await _casas.ListarTodasAsync()).ToList();
        var lancamentosMesReceita = await _financeiro.SomarAsync(TipoLancamento.Receita, null, inicioMes, fimMes);
        var lancamentosMesDespesa = await _financeiro.SomarAsync(TipoLancamento.Despesa, null, inicioMes, fimMes);
        var recebido = await _financeiro.SomarAsync(TipoLancamento.Receita, StatusFinanceiro.Pago, inicioMes, fimMes);
        var pendente = await _financeiro.SomarAsync(TipoLancamento.Receita, StatusFinanceiro.Pendente, inicioMes, fimMes);
        var atrasado = await _financeiro.SomarAsync(TipoLancamento.Receita, StatusFinanceiro.Atrasado, new DateTime(2000, 1, 1), fimMes);
        var vencimentos = (await _financeiro.ListarVencimentosAsync(20)).ToList();

        var cards = new List<CardCasaDashboardViewModel>();
        foreach (var casa in listaCasas)
        {
            var lancs = (await _financeiro.ListarPorCasaAsync(casa.Id)).ToList();
            var situacao = "Em dia";
            if (lancs.Any(l => l.Tipo == TipoLancamento.Receita && l.Status == StatusFinanceiro.Atrasado))
            {
                situacao = "Atrasado";
            }
            else if (lancs.Any(l => l.Tipo == TipoLancamento.Receita && l.Status == StatusFinanceiro.Pendente))
            {
                situacao = "Pendente";
            }
            else if (casa.Status == StatusCasa.Disponivel)
            {
                situacao = "Disponível";
            }

            cards.Add(new CardCasaDashboardViewModel
            {
                Id = casa.Id,
                Nome = casa.Nome,
                Endereco = $"{casa.Endereco}, {casa.Numero} - {casa.Bairro}",
                Status = casa.Status,
                InquilinoAtual = casa.InquilinoAtual,
                ValorAluguel = casa.ValorAluguel,
                DiaVencimento = casa.DiaVencimento,
                SituacaoFinanceira = situacao
            });
        }

        return new DashboardViewModel
        {
            TotalCasas = listaCasas.Count,
            CasasAlugadas = listaCasas.Count(c => c.Status == StatusCasa.Alugada),
            CasasDisponiveis = listaCasas.Count(c => c.Status == StatusCasa.Disponivel),
            CasasManutencao = listaCasas.Count(c => c.Status == StatusCasa.Manutencao),
            TotalReceberMes = listaCasas.Where(c => c.Status == StatusCasa.Alugada).Sum(c => c.ValorAluguel),
            TotalRecebidoMes = recebido,
            TotalPendenteMes = pendente,
            TotalAtrasado = atrasado,
            QtdAlugueisAtrasados = vencimentos.Count(v => v.Origem == OrigemReceita.Aluguel && v.Status == StatusFinanceiro.Atrasado),
            DespesasMes = lancamentosMesDespesa,
            ReceitasMes = lancamentosMesReceita,
            SaldoMes = lancamentosMesReceita - lancamentosMesDespesa,
            Casas = cards,
            ProximosVencimentos = vencimentos.Select(v => new VencimentoDashboardViewModel
            {
                Casa = v.CasaNome ?? "-",
                Inquilino = v.InquilinoNome ?? "-",
                Tipo = string.IsNullOrWhiteSpace(v.Origem) ? v.Tipo : v.Origem,
                Vencimento = v.Vencimento,
                Valor = v.Valor,
                Status = v.Status
            }),
            ContratosProximosVencimento = await _contratos.ListarProximosVencimentoAsync(60)
        };
    }
}
