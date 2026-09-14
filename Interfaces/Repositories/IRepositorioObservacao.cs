using SistemaAlugueis.Models;

namespace SistemaAlugueis.Interfaces.Repositories;

public interface IRepositorioObservacao
{
    Task<IEnumerable<Observacao>> ListarAsync(string? tipo, int? casaId, string? busca);
    Task<Observacao?> ObterPorIdAsync(int id);
    Task<IEnumerable<Observacao>> ListarPorCasaAsync(int casaId);
    Task<IEnumerable<Observacao>> ListarPorInquilinoAsync(int inquilinoId);
    Task<int> InserirAsync(Observacao observacao);
    Task AtualizarAsync(Observacao observacao);
    Task ExcluirAsync(int id);
}
