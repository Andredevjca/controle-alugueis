using SistemaAlugueis.Interfaces.Services;
using System.Globalization;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoObservacao : IServicoObservacao
{
    private readonly IRepositorioObservacao _repositorio;

    public ServicoObservacao(
        IRepositorioObservacao repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<IEnumerable<Observacao>> ListarAsync(string? tipo, int? casaId, string? busca)
        => _repositorio.ListarAsync(tipo, casaId, busca);

    public Task<Observacao?> ObterAsync(int id) => _repositorio.ObterPorIdAsync(id);

    public async Task<int> SalvarAsync(ObservacaoViewModel modelo, int? usuarioId)
    {
        var entidade = new Observacao
        {
            Id = modelo.Id,
            Titulo = modelo.Titulo,
            Descricao = modelo.Descricao,
            Tipo = modelo.Tipo,
            CasaId = modelo.CasaId,
            ContratoId = modelo.ContratoId,
            InquilinoId = modelo.InquilinoId,
            DataObservacao = DateTime.Now,
            UsuarioId = usuarioId
        };

        if (modelo.Id == 0)
        {
            return await _repositorio.InserirAsync(entidade);
        }

        await _repositorio.AtualizarAsync(entidade);
        return modelo.Id;
    }

    public Task ExcluirAsync(int id) => _repositorio.ExcluirAsync(id);
}
