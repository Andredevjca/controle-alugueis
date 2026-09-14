using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoConfiguracao
{
    Task<ConfiguracaoViewModel> ObterAsync();
    Task AtualizarAsync(int id, string? valor);
}
