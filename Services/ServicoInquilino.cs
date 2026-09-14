using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoInquilino : IServicoInquilino
{
    private readonly IRepositorioInquilino _inquilino;
    private readonly IRepositorioContrato _contratos;
    private readonly IRepositorioCasa _casas;
    private readonly IRepositorioFinanceiro _financeiro;
    private readonly IRepositorioObservacao _observacoes;

    public ServicoInquilino(IRepositorioInquilino inquilino, IRepositorioContrato contratos, IRepositorioCasa casas, IRepositorioFinanceiro financeiro, IRepositorioObservacao observacoes)
    {
        _inquilino = inquilino;
        _contratos = contratos;
        _casas = casas;
        _financeiro = financeiro;
        _observacoes = observacoes;
    }

    public Task<(IEnumerable<Inquilino> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanho)
        => _inquilino.ListarAsync(busca, pagina, tamanho);

    public Task<IEnumerable<Inquilino>> ListarTodosAsync() => _inquilino.ListarTodosAsync();

    public Task<Inquilino?> ObterAsync(int id) => _inquilino.ObterPorIdAsync(id);

    public async Task<int> SalvarAsync(InquilinoViewModel modelo)
    {
        var entidade = Mapear(modelo);
        if (modelo.Id == 0)
        {
            return await _inquilino.InserirAsync(entidade);
        }

        await _inquilino.AtualizarAsync(entidade);
        return modelo.Id;
    }

    public async Task ExcluirAsync(int id)
    {
        var ativo = await _contratos.ObterAtivoPorInquilinoAsync(id);
        if (ativo != null)
        {
            throw new InvalidOperationException("Não é possível excluir um inquilino com contrato ativo.");
        }

        await _inquilino.ExcluirAsync(id);
    }

    public async Task<InquilinoDetalhesViewModel> ObterDetalhesAsync(int id)
    {
        var inquilino = await _inquilino.ObterPorIdAsync(id) ?? throw new InvalidOperationException("Inquilino não encontrado.");
        var historico = (await _contratos.ListarPorInquilinoAsync(id)).ToList();
        var atual = historico.FirstOrDefault(c => c.Status == StatusContrato.Ativo);
        Casa? casa = null;
        if (atual != null)
        {
            casa = await _casas.ObterPorIdAsync(atual.CasaId);
        }

        return new InquilinoDetalhesViewModel
        {
            Inquilino = inquilino,
            ContratoAtual = atual,
            CasaAtual = casa,
            TempoResidencia = atual == null ? "-" : Formatador.TempoResidencia(atual.DataInicio),
            HistoricoContratos = historico.Select(c => new HistoricoInquilinoViewModel
            {
                ContratoId = c.Id,
                InquilinoId = c.InquilinoId,
                NomeInquilino = c.CasaNome ?? "-",
                NumeroContrato = c.Numero,
                DataEntrada = c.DataInicio,
                DataSaida = c.DataFim,
                ValorAluguel = c.ValorAluguel,
                Status = c.Status,
                Atual = c.Status == StatusContrato.Ativo,
                TempoResidencia = Formatador.TempoResidencia(c.DataInicio, c.DataFim)
            }),
            HistoricoFinanceiro = await _financeiro.ListarPorInquilinoAsync(id),
            Observacoes = await _observacoes.ListarPorInquilinoAsync(id)
        };
    }

    public static InquilinoViewModel ParaFormulario(Inquilino i) => new()
    {
        Id = i.Id,
        NomeCompleto = i.NomeCompleto,
        Cpf = i.Cpf,
        Rg = i.Rg,
        DataNascimento = i.DataNascimento,
        Telefone = i.Telefone,
        Whatsapp = i.Whatsapp,
        Email = i.Email,
        Profissao = i.Profissao,
        Renda = i.Renda,
        EnderecoAnterior = i.EnderecoAnterior,
        Observacoes = i.Observacoes
    };

    private static Inquilino Mapear(InquilinoViewModel m) => new()
    {
        Id = m.Id,
        NomeCompleto = m.NomeCompleto,
        Cpf = m.Cpf,
        Rg = m.Rg,
        DataNascimento = m.DataNascimento,
        Telefone = m.Telefone,
        Whatsapp = m.Whatsapp,
        Email = m.Email,
        Profissao = m.Profissao,
        Renda = m.Renda,
        EnderecoAnterior = m.EnderecoAnterior,
        Observacoes = m.Observacoes
    };
}

public class ServicoContrato : IServicoContrato
{
    private readonly IRepositorioContrato _repositorio;
    private readonly IRepositorioCasa _casas;

    public ServicoContrato(
        IRepositorioContrato repositorio,
        IRepositorioCasa casas)
    {
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
}
