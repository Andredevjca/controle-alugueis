using SistemaAlugueis.Models;

namespace SistemaAlugueis.Interfaces.Repositories;

public interface IRepositorioContaConsumo
{
    Task<(IEnumerable<ContaConsumo> Itens, int Total)> ListarAsync(string? tipo, int? casaId, string? status, string? busca, int pagina, int tamanho);
    Task<ContaConsumo?> ObterPorIdAsync(int id);
    Task<IEnumerable<ContaConsumo>> ListarPorCasaAsync(int casaId, string? tipo = null);
    Task<int> InserirAsync(ContaConsumo conta);
    Task AtualizarAsync(ContaConsumo conta);
    Task CancelarAsync(int id);
    Task MarcarPagoAsync(int id, DateTime dataPagamento);
    Task MarcarAtrasadosAsync();
}
