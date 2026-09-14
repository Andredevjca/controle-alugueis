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

public class ServicoRelatorio : IServicoRelatorio
{
    private readonly IRepositorioCasa _casas;
    private readonly IRepositorioContrato _contratos;
    private readonly IRepositorioFinanceiro _financeiro;
    private readonly IRepositorioContaConsumo _consumo;

    public ServicoRelatorio(
        IRepositorioCasa casas,
        IRepositorioContrato contratos,
        IRepositorioFinanceiro financeiro,
        IRepositorioContaConsumo consumo)
    {
        _casas = casas;
        _contratos = contratos;
        _financeiro = financeiro;
        _consumo = consumo;
    }

    public async Task<RelatorioViewModel> GerarAsync(RelatorioViewModel filtro)
    {
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

public class ServicoAutenticacao : IServicoAutenticacao
{
    private readonly IRepositorioUsuario _usuarios;

    public ServicoAutenticacao(
        IRepositorioUsuario usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task GarantirAdministradorAsync()
    {
        try
        {
            if (await _usuarios.ContarAsync() > 0)
            {
                return;
            }

            await _usuarios.InserirAsync(new Models.Usuario
            {
                Nome = "Administrador",
                Email = "admin@sistema.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Perfil = PerfilUsuario.Administrador,
                Ativo = true
            });
        }
        catch
        {
            // Banco ainda não disponível na inicialização.
        }
    }

    public async Task<Models.Usuario?> ValidarAsync(string email, string senha)
    {
        var usuario = await _usuarios.ObterPorEmailAsync(email);
        if (usuario == null || !usuario.Ativo)
        {
            return null;
        }

        return BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash) ? usuario : null;
    }
}
