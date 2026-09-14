using SistemaAlugueis.Models;

namespace SistemaAlugueis.Interfaces.Repositories;

public interface IRepositorioCategoria
{
    Task<IEnumerable<CategoriaFinanceira>> ListarAsync(string? tipo = null);
    Task<CategoriaFinanceira?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(CategoriaFinanceira categoria);
    Task AtualizarAsync(CategoriaFinanceira categoria);
}
