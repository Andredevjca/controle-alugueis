using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoConfiguracao : IServicoConfiguracao
{
    private readonly IRepositorioConfiguracao _repositorio;

    public ServicoConfiguracao(IRepositorioConfiguracao repositorio)
    {
        _repositorio = repositorio;
    }
    public async Task<ConfiguracaoViewModel> ObterAsync() => new() { Itens = await _repositorio.ListarAsync() };
    public Task AtualizarAsync(int id, string? valor) => _repositorio.AtualizarAsync(id, valor);
}
