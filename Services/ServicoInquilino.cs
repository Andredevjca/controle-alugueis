using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;
using SistemaAlugueis.Repositories;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoInquilino(IRepositorioInquilino repositorio, IRepositorioContrato contratos,
    IRepositorioCasa casas, IRepositorioFinanceiro financeiro, IRepositorioObservacao observacoes)
{
    public Task<(IEnumerable<Inquilino> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanho)
        => repositorio.ListarAsync(busca, pagina, tamanho);

    public Task<IEnumerable<Inquilino>> ListarTodosAsync() => repositorio.ListarTodosAsync();

    public Task<Inquilino?> ObterAsync(int id) => repositorio.ObterPorIdAsync(id);

    public async Task<int> SalvarAsync(InquilinoViewModel modelo)
    {
        var entidade = Mapear(modelo);
        if (modelo.Id == 0)
        {
            return await repositorio.InserirAsync(entidade);
        }

        await repositorio.AtualizarAsync(entidade);
        return modelo.Id;
    }

    public async Task ExcluirAsync(int id)
    {
        var ativo = await contratos.ObterAtivoPorInquilinoAsync(id);
        if (ativo != null)
        {
            throw new InvalidOperationException("Não é possível excluir um inquilino com contrato ativo.");
        }

        await repositorio.ExcluirAsync(id);
    }

    public async Task<InquilinoDetalhesViewModel> ObterDetalhesAsync(int id)
    {
        var inquilino = await repositorio.ObterPorIdAsync(id) ?? throw new InvalidOperationException("Inquilino não encontrado.");
        var historico = (await contratos.ListarPorInquilinoAsync(id)).ToList();
        var atual = historico.FirstOrDefault(c => c.Status == StatusContrato.Ativo);
        Casa? casa = null;
        if (atual != null)
        {
            casa = await casas.ObterPorIdAsync(atual.CasaId);
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
            HistoricoFinanceiro = await financeiro.ListarPorInquilinoAsync(id),
            Observacoes = await observacoes.ListarPorInquilinoAsync(id)
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

public class ServicoContrato(IRepositorioContrato repositorio, IRepositorioCasa casas)
{
    public Task<(IEnumerable<Contrato> Itens, int Total)> ListarAsync(string? busca, string? status, int pagina, int tamanho)
        => repositorio.ListarAsync(busca, status, pagina, tamanho);

    public Task<IEnumerable<Contrato>> ListarTodosAsync() => repositorio.ListarTodosAsync();

    public Task<Contrato?> ObterAsync(int id) => repositorio.ObterPorIdAsync(id);

    public async Task<int> SalvarAsync(ContratoViewModel modelo)
    {
        var casaAtiva = await repositorio.ObterAtivoPorCasaAsync(modelo.CasaId);
        if (casaAtiva != null && casaAtiva.Id != modelo.Id)
        {
            throw new InvalidOperationException("Esta casa já possui um contrato ativo.");
        }

        var inquilinoAtivo = await repositorio.ObterAtivoPorInquilinoAsync(modelo.InquilinoId);
        if (inquilinoAtivo != null && inquilinoAtivo.Id != modelo.Id)
        {
            throw new InvalidOperationException("Este inquilino já possui um contrato ativo.");
        }

        var entidade = Mapear(modelo);
        entidade.Status = StatusContrato.Ativo;

        if (modelo.Id == 0)
        {
            var id = await repositorio.InserirAsync(entidade);
            await casas.AtualizarStatusAsync(modelo.CasaId, StatusCasa.Alugada);
            return id;
        }

        await repositorio.AtualizarAsync(entidade);
        return modelo.Id;
    }

    public async Task EncerrarAsync(int id, DateTime dataSaida, string status)
    {
        var contrato = await repositorio.ObterPorIdAsync(id) ?? throw new InvalidOperationException("Contrato não encontrado.");
        await repositorio.EncerrarAsync(id, dataSaida, status);
        var outroAtivo = await repositorio.ObterAtivoPorCasaAsync(contrato.CasaId);
        if (outroAtivo == null)
        {
            var casa = await casas.ObterPorIdAsync(contrato.CasaId);
            if (casa != null && casa.Status != StatusCasa.Manutencao)
            {
                await casas.AtualizarStatusAsync(contrato.CasaId, StatusCasa.Disponivel);
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
