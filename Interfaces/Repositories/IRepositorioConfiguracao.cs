using SistemaAlugueis.Models;

namespace SistemaAlugueis.Interfaces.Repositories;

public interface IRepositorioConfiguracao
{
    Task<IEnumerable<Configuracao>> ListarAsync();
    Task AtualizarAsync(int id, string? valor);
}
