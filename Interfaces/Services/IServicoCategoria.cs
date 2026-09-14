using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoCategoria
{
    Task<IEnumerable<CategoriaFinanceira>> ListarAsync();
    Task<int> CriarAsync(CategoriaViewModel modelo);
}
