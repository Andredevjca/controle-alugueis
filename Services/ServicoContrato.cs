using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

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
