using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoObservacao
{
    Task<IEnumerable<Observacao>> ListarAsync(string? tipo, int? casaId, string? busca);
    Task<Observacao?> ObterAsync(int id);
    Task<int> SalvarAsync(ObservacaoViewModel modelo, int? usuarioId);
    Task ExcluirAsync(int id);
}
