using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoFinanceiro
{
    Task<(IEnumerable<LancamentoFinanceiro> Itens, int Total)> ListarAsync(
        string? tipo, string? status, int? casaId, int? categoriaId, DateTime? inicio, DateTime? fim, string? busca, int pagina, int tamanho);
    Task<LancamentoFinanceiro?> ObterAsync(int id);
    Task<IEnumerable<CategoriaFinanceira>> ListarCategoriasAsync(string? tipo = null);
    Task<int> SalvarAsync(FinanceiroViewModel modelo);
    Task CancelarAsync(int id);
    Task MarcarPagoAsync(int id);
    Task<FinanceiroListaViewModel> ObterListaAsync(string? tipo, string? status, int? casaId, int? categoriaId,
        DateTime? dataInicio, DateTime? dataFim, string? busca, int pagina = 1);
    Task<FinanceiroViewModel> PrepararFormularioAsync(FinanceiroViewModel modelo);
    Task<FinanceiroViewModel?> ObterFormularioAsync(int id);
    Task<FinanceiroViewModel> NovoFormularioAsync(string tipo, int? casaId);
}
