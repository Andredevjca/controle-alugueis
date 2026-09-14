using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoRelatorio
{
    Task<RelatorioViewModel> GerarAsync(RelatorioViewModel filtro);
}
