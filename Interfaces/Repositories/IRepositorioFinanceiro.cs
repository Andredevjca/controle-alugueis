using SistemaAlugueis.Models;

namespace SistemaAlugueis.Interfaces.Repositories;

public interface IRepositorioFinanceiro
{
    Task<(IEnumerable<LancamentoFinanceiro> Itens, int Total)> ListarAsync(
        string? tipo, string? status, int? casaId, int? categoriaId, DateTime? inicio, DateTime? fim, string? busca, int pagina, int tamanho);
    Task<LancamentoFinanceiro?> ObterPorIdAsync(int id);
    Task<IEnumerable<LancamentoFinanceiro>> ListarPorCasaAsync(int casaId);
    Task<IEnumerable<LancamentoFinanceiro>> ListarPorInquilinoAsync(int inquilinoId);
    Task<int> InserirAsync(LancamentoFinanceiro lancamento);
    Task AtualizarAsync(LancamentoFinanceiro lancamento);
    Task CancelarAsync(int id);
    Task MarcarPagoAsync(int id, DateTime dataPagamento);
    Task MarcarAtrasadosAsync();
    Task<bool> ExisteAluguelDoMesAsync(int? casaId, DateTime vencimento);
    Task<decimal> SomarAsync(string tipo, string? status, DateTime inicio, DateTime fim, string? origem = null);
    Task<IEnumerable<LancamentoFinanceiro>> ListarVencimentosAsync(int dias);
}
