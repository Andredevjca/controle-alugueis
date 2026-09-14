using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoDashboard
{
    Task<DashboardViewModel> ObterAsync();
}
