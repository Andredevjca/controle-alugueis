using SistemaAlugueis.Models;

namespace SistemaAlugueis.Interfaces.Repositories;

public interface IRepositorioCasa
{
    Task<(IEnumerable<Casa> Itens, int Total)> ListarAsync(string? busca, string? status, int pagina, int tamanho);
    Task<IEnumerable<Casa>> ListarTodasAsync();
    Task<Casa?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(Casa casa);
    Task AtualizarAsync(Casa casa);
    Task ExcluirAsync(int id);
    Task AtualizarStatusAsync(int id, string status);
    Task<int> ContarAsync(string? status = null);
}
