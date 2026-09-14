using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoContaConsumo
{
    Task<(IEnumerable<ContaConsumo> Itens, int Total)> ListarAsync(string? tipo, int? casaId, string? status, string? busca, int pagina, int tamanho);
    Task<ContaConsumo?> ObterAsync(int id);
    Task<int> SalvarAsync(ContaConsumoViewModel modelo);
    Task CancelarAsync(int id);
    Task MarcarPagoAsync(int id);
}
