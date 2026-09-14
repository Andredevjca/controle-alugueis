using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoRelatorio : IServicoRelatorio
{
    private readonly IRepositorioCasa _casas;
    private readonly IRepositorioContrato _contratos;
    private readonly IRepositorioFinanceiro _financeiro;
    private readonly IRepositorioContaConsumo _consumo;

    private readonly IRepositorioInquilino _inquilinos;

    public ServicoRelatorio(
        IRepositorioCasa casas,
        IRepositorioContrato contratos,
        IRepositorioFinanceiro financeiro,
        IRepositorioContaConsumo consumo, IRepositorioInquilino inquilinos)
    {
        _inquilinos = inquilinos;
        _casas = casas;
        _contratos = contratos;
        _financeiro = financeiro;
        _consumo = consumo;
    }

    public async Task<RelatorioViewModel> GerarAsync(RelatorioViewModel filtro)
    {
        filtro.TipoRelatorio = string.IsNullOrWhiteSpace(filtro.TipoRelatorio) ? "casas" : filtro.TipoRelatorio;

        filtro.Casas = (await _casas.ListarTodasAsync()).Select(c => new SelectListItem(c.Nome, c.Id.ToString(), c.Id == filtro.CasaId));

        filtro.Inquilinos = (await _inquilinos.ListarTodosAsync()).Select(i => new SelectListItem(i.NomeCompleto, i.Id.ToString(), i.Id == filtro.InquilinoId));

        var inicio = filtro.DataInicio ?? new DateTime(DateTime.Today.Year, 1, 1);
        var fim = filtro.DataFim ?? DateTime.Today;
        filtro.Titulo = Titulo(filtro.TipoRelatorio);

        switch (filtro.TipoRelatorio)
        {
            case "casas":
            case "casas-alugadas":
            case "casas-disponiveis":
                var statusCasa = filtro.TipoRelatorio switch
                {
                    "casas-alugadas" => StatusCasa.Alugada,
                    "casas-disponiveis" => StatusCasa.Disponivel,
                    _ => filtro.Status
                };
                var listaCasas = await _casas.ListarTodasAsync();
                var filtradas = listaCasas.Where(c =>
                    (statusCasa == null || c.Status == statusCasa) &&
                    (!filtro.CasaId.HasValue || c.Id == filtro.CasaId));
                filtro.Colunas = ["Casa", "Endereço", "Valor", "Inquilino", "Status"];
                filtro.Linhas = filtradas.Select(c => Dict(
                    ("Casa", c.Nome),
                    ("Endereço", $"{c.Endereco}, {c.Numero}"),
                    ("Valor", Formatador.Moeda(c.ValorAluguel)),
                    ("Inquilino", c.InquilinoAtual ?? "-"),
                    ("Status", c.Status)));
                break;

            case "inquilinos-atuais":
                var contratosAtivos = (await _contratos.ListarTodosAsync())
                    .Where(c => c.Status == StatusContrato.Ativo)
                    .Where(c => !filtro.InquilinoId.HasValue || c.InquilinoId == filtro.InquilinoId);
                filtro.Colunas = ["Inquilino", "Casa", "Contrato", "Entrada", "Aluguel"];
                filtro.Linhas = contratosAtivos.Select(c => Dict(
                    ("Inquilino", c.InquilinoNome),
                    ("Casa", c.CasaNome),
                    ("Contrato", c.Numero),
                    ("Entrada", Formatador.Data(c.DataInicio)),
                    ("Aluguel", Formatador.Moeda(c.ValorAluguel))));
                break;

            case "historico-inquilinos":
                var todosContratos = await _contratos.ListarTodosAsync();
                if (filtro.InquilinoId.HasValue)
                {
                    todosContratos = todosContratos.Where(c => c.InquilinoId == filtro.InquilinoId);
                }
                filtro.Colunas = ["Inquilino", "Casa", "Contrato", "Entrada", "Saída", "Status"];
                filtro.Linhas = todosContratos.Select(c => Dict(
                    ("Inquilino", c.InquilinoNome),
                    ("Casa", c.CasaNome),
                    ("Contrato", c.Numero),
                    ("Entrada", Formatador.Data(c.DataInicio)),
                    ("Saída", Formatador.Data(c.DataFim)),
                    ("Status", c.Status)));
                break;

            case "contratos-ativos":
            case "contratos-encerrados":
                var st = filtro.TipoRelatorio == "contratos-ativos" ? StatusContrato.Ativo : StatusContrato.Encerrado;
                var listaC = (await _contratos.ListarTodosAsync()).Where(c => c.Status == st);
                filtro.Colunas = ["Número", "Casa", "Inquilino", "Início", "Término", "Valor", "Status"];
                filtro.Linhas = listaC.Select(c => Dict(
                    ("Número", c.Numero),
                    ("Casa", c.CasaNome),
                    ("Inquilino", c.InquilinoNome),
                    ("Início", Formatador.Data(c.DataInicio)),
                    ("Término", Formatador.Data(c.DataTermino)),
                    ("Valor", Formatador.Moeda(c.ValorAluguel)),
                    ("Status", c.Status)));
                break;

            case "alugueis-recebidos":
            case "alugueis-pendentes":
            case "alugueis-atrasados":
            case "receitas":
            case "despesas":
            case "saldo":
                var tipo = filtro.TipoRelatorio == "despesas" ? TipoLancamento.Despesa : TipoLancamento.Receita;
                var statusFin = filtro.TipoRelatorio switch
                {
                    "alugueis-recebidos" => StatusFinanceiro.Pago,
                    "alugueis-pendentes" => StatusFinanceiro.Pendente,
                    "alugueis-atrasados" => StatusFinanceiro.Atrasado,
                    _ => filtro.Status
                };
                var origem = filtro.TipoRelatorio.StartsWith("alugueis") ? OrigemReceita.Aluguel : null;
                var (itens, _) = await _financeiro.ListarAsync(tipo, statusFin, filtro.CasaId, null, inicio, fim, null, 1, 500);
                if (origem != null)
                {
                    itens = itens.Where(i => i.Origem == origem);
                }

                filtro.Colunas = ["Data", "Casa", "Tipo", "Descrição", "Vencimento", "Valor", "Status"];
                filtro.Linhas = itens.Select(i => Dict(
                    ("Data", Formatador.Data(i.DataLancamento)),
                    ("Casa", i.CasaNome ?? "-"),
                    ("Tipo", i.Tipo),
                    ("Descrição", i.Descricao),
                    ("Vencimento", Formatador.Data(i.Vencimento)),
                    ("Valor", Formatador.Moeda(i.Valor)),
                    ("Status", i.Status)));
                filtro.Total = itens.Where(i => i.Status != StatusFinanceiro.Cancelado).Sum(i => i.Valor);
                if (filtro.TipoRelatorio == "saldo")
                {
                    var rec = await _financeiro.SomarAsync(TipoLancamento.Receita, null, inicio, fim);
                    var des = await _financeiro.SomarAsync(TipoLancamento.Despesa, null, inicio, fim);
                    filtro.Total = rec - des;
                }
                break;

            case "contas-agua":
            case "contas-luz":
                var tipoC = filtro.TipoRelatorio == "contas-agua" ? TipoConsumo.Agua : TipoConsumo.Luz;
                var (contas, _) = await _consumo.ListarAsync(tipoC, filtro.CasaId, filtro.Status, null, 1, 500);
                filtro.Colunas = ["Casa", "Tipo", "Referência", "Consumo", "Valor", "Vencimento", "Status"];
                filtro.Linhas = contas.Select(c => Dict(
                    ("Casa", c.CasaNome ?? "-"),
                    ("Tipo", c.Tipo),
                    ("Referência", c.Referencia),
                    ("Consumo", c.Consumo?.ToString("N2") ?? "-"),
                    ("Valor", Formatador.Moeda(c.Valor)),
                    ("Vencimento", Formatador.Data(c.Vencimento)),
                    ("Status", c.Status)));
                filtro.Total = contas.Sum(c => c.Valor);
                break;
        }

        return filtro;
    }

    private static string Titulo(string tipo) => tipo switch
    {
        "casas" => "Casas cadastradas",
        "casas-alugadas" => "Casas alugadas",
        "casas-disponiveis" => "Casas disponíveis",
        "inquilinos-atuais" => "Inquilinos atuais",
        "historico-inquilinos" => "Histórico de inquilinos",
        "contratos-ativos" => "Contratos ativos",
        "contratos-encerrados" => "Contratos encerrados",
        "alugueis-recebidos" => "Aluguéis recebidos",
        "alugueis-pendentes" => "Aluguéis pendentes",
        "alugueis-atrasados" => "Aluguéis atrasados",
        "receitas" => "Receitas",
        "despesas" => "Despesas",
        "saldo" => "Saldo",
        "contas-agua" => "Contas de água",
        "contas-luz" => "Contas de energia",
        _ => "Relatórios"
    };

    private static Dictionary<string, object?> Dict(params (string K, object? V)[] pares)
        => pares.ToDictionary(p => p.K, p => p.V);
}
