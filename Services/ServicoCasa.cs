using SistemaAlugueis.Helpers;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoCasa : IServicoCasa
{
    private readonly IRepositorioCasa _casa;
    private readonly IRepositorioContrato _contratos;
    private readonly IRepositorioFinanceiro _financeiro;
    private readonly IRepositorioContaConsumo _consumo;
    private readonly IRepositorioObservacao _observacoes;
    private readonly IRepositorioInquilino _inquilinos;

    public ServicoCasa(IRepositorioCasa casa, IRepositorioContrato contratos, IRepositorioFinanceiro financeiro, IRepositorioContaConsumo consumo, IRepositorioObservacao observacoes, IRepositorioInquilino inquilinos)
    {
        _casa = casa;
        _contratos = contratos;
        _financeiro = financeiro;
        _consumo = consumo;
        _observacoes = observacoes;
        _inquilinos = inquilinos;
    }

    public Task<(IEnumerable<Casa> Itens, int Total)> ListarAsync(string? busca, string? status, int pagina, int tamanho)
        => _casa.ListarAsync(busca, status, pagina, tamanho);

    public Task<IEnumerable<Casa>> ListarTodasAsync() => _casa.ListarTodasAsync();

    public Task<Casa?> ObterAsync(int id) => _casa.ObterPorIdAsync(id);

    public async Task<int> SalvarAsync(CasaViewModel modelo)
    {
        var entidade = Mapear(modelo);
        if (modelo.Id == 0)
        {
            return await _casa.InserirAsync(entidade);
        }

        var atual = await _casa.ObterPorIdAsync(modelo.Id) ?? throw new InvalidOperationException("Casa não encontrada.");
        if (atual.Status == StatusCasa.Alugada && entidade.Status == StatusCasa.Disponivel)
        {
            var ativo = await _contratos.ObterAtivoPorCasaAsync(modelo.Id);
            if (ativo != null)
            {
                throw new InvalidOperationException("Não é possível marcar a casa como disponível enquanto houver contrato ativo.");
            }
        }

        await _casa.AtualizarAsync(entidade);
        return modelo.Id;
    }

    public async Task ExcluirAsync(int id)
    {
        var ativo = await _contratos.ObterAtivoPorCasaAsync(id);
        if (ativo != null)
        {
            throw new InvalidOperationException("Não é possível excluir uma casa com contrato ativo. Encerre o contrato primeiro.");
        }

        await _casa.ExcluirAsync(id);
    }

    public async Task<CasaDetalhesViewModel> ObterDetalhesAsync(int id, string aba = "resumo")
    {
        var casa = await _casa.ObterPorIdAsync(id) ?? throw new InvalidOperationException("Casa não encontrada.");
        var historicoContratos = (await _contratos.ListarPorCasaAsync(id)).ToList();
        var contratoAtual = historicoContratos.FirstOrDefault(c => c.Status == StatusContrato.Ativo);
        Inquilino? inquilino = null;
        if (contratoAtual != null)
        {
            inquilino = await _inquilinos.ObterPorIdAsync(contratoAtual.InquilinoId);
        }

        var lancamentos = (await _financeiro.ListarPorCasaAsync(id)).ToList();
        var contas = (await _consumo.ListarPorCasaAsync(id)).ToList();
        var contasAgua = contas.Where(c => c.Tipo == TipoConsumo.Agua).ToList();
        var contasLuz = contas.Where(c => c.Tipo == TipoConsumo.Luz).ToList();

        var situacao = "Em dia";
        if (lancamentos.Any(l => l.Tipo == TipoLancamento.Receita && l.Status == StatusFinanceiro.Atrasado))
        {
            situacao = "Atrasado";
        }
        else if (lancamentos.Any(l => l.Tipo == TipoLancamento.Receita && l.Status == StatusFinanceiro.Pendente))
        {
            situacao = "Pendente";
        }

        DateTime? proximo = null;
        if (contratoAtual != null)
        {
            var hoje = DateTime.Today;
            var dia = Math.Min(contratoAtual.DiaVencimento, DateTime.DaysInMonth(hoje.Year, hoje.Month));
            proximo = new DateTime(hoje.Year, hoje.Month, dia);
            if (proximo < hoje)
            {
                var proximoMes = hoje.AddMonths(1);
                dia = Math.Min(contratoAtual.DiaVencimento, DateTime.DaysInMonth(proximoMes.Year, proximoMes.Month));
                proximo = new DateTime(proximoMes.Year, proximoMes.Month, dia);
            }
        }

        return new CasaDetalhesViewModel
        {
            Casa = casa,
            ContratoAtual = contratoAtual,
            InquilinoAtual = inquilino,
            TempoResidencia = contratoAtual == null ? "-" : Formatador.TempoResidencia(contratoAtual.DataInicio),
            ProximoVencimento = proximo,
            SituacaoFinanceira = situacao,
            Lancamentos = lancamentos,
            ContasAgua = contasAgua,
            ContasLuz = contasLuz,
            Observacoes = await _observacoes.ListarPorCasaAsync(id),
            Historico = historicoContratos.Select(c => new HistoricoInquilinoViewModel
            {
                ContratoId = c.Id,
                InquilinoId = c.InquilinoId,
                NomeInquilino = c.InquilinoNome ?? "-",
                NumeroContrato = c.Numero,
                DataEntrada = c.DataInicio,
                DataSaida = c.DataFim,
                ValorAluguel = c.ValorAluguel,
                Status = c.Status,
                Atual = c.Status == StatusContrato.Ativo,
                TempoResidencia = Formatador.TempoResidencia(c.DataInicio, c.DataFim)
            }),
            MediaConsumoAgua = contasAgua.Count == 0 ? 0 : contasAgua.Average(x => x.Consumo ?? 0),
            MediaValorAgua = contasAgua.Count == 0 ? 0 : contasAgua.Average(x => x.Valor),
            MediaConsumoLuz = contasLuz.Count == 0 ? 0 : contasLuz.Average(x => x.Consumo ?? 0),
            MediaValorLuz = contasLuz.Count == 0 ? 0 : contasLuz.Average(x => x.Valor),
            UltimaAgua = contasAgua.FirstOrDefault(),
            UltimaLuz = contasLuz.FirstOrDefault(),
            Aba = aba
        };
    }

    public static CasaViewModel ParaFormulario(Casa casa) => new()
    {
        Id = casa.Id,
        Nome = casa.Nome,
        Cep = casa.Cep,
        Endereco = casa.Endereco,
        Numero = casa.Numero,
        Complemento = casa.Complemento,
        Bairro = casa.Bairro,
        Cidade = casa.Cidade,
        Estado = casa.Estado,
        ValorAluguel = casa.ValorAluguel,
        DiaVencimento = casa.DiaVencimento,
        AreaM2 = casa.AreaM2,
        QtdQuartos = casa.QtdQuartos,
        QtdBanheiros = casa.QtdBanheiros,
        QtdVagas = casa.QtdVagas,
        Status = casa.Status,
        Observacoes = casa.Observacoes
    };

    private static Casa Mapear(CasaViewModel m) => new()
    {
        Id = m.Id,
        Nome = m.Nome,
        Cep = m.Cep,
        Endereco = m.Endereco,
        Numero = m.Numero,
        Complemento = m.Complemento,
        Bairro = m.Bairro,
        Cidade = m.Cidade,
        Estado = m.Estado?.ToUpperInvariant() ?? "",
        ValorAluguel = m.ValorAluguel,
        DiaVencimento = m.DiaVencimento,
        AreaM2 = m.AreaM2,
        QtdQuartos = m.QtdQuartos,
        QtdBanheiros = m.QtdBanheiros,
        QtdVagas = m.QtdVagas,
        Status = m.Status,
        Observacoes = m.Observacoes
    };

    public async Task<CasaListaViewModel> ObterListaAsync(string? busca, string? status, int pagina = 1)
    {
        const int tamanho = 10;

        var (itens, total) = await ListarAsync(busca, status, pagina, tamanho);

        return new CasaListaViewModel
        {
            Busca = busca,
            Status = status,
            Pagina = pagina,
            TotalRegistros = total,
            TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanho)),
            Itens = itens.Select(c => new CasaListaItemViewModel
            {
                Id = c.Id,
                Nome = c.Nome,
                EnderecoCompleto = $"{c.Endereco}, {c.Numero} - {c.Bairro}, {c.Cidade}/{c.Estado}",
                ValorAluguel = c.ValorAluguel,
                InquilinoAtual = c.InquilinoAtual,
                Status = c.Status
            })
        };
    }

    public async Task<CasaViewModel?> ObterFormularioAsync(int id)
    {
        var entidade = await ObterAsync(id);
        if (entidade == null) return null;
        return ParaFormulario(entidade);
    }
}
