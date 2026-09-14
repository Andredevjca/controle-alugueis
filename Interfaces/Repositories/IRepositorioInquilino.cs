using SistemaAlugueis.Models;

namespace SistemaAlugueis.Interfaces.Repositories;

public interface IRepositorioInquilino
{
    Task<(IEnumerable<Inquilino> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanho);
    Task<IEnumerable<Inquilino>> ListarTodosAsync();
    Task<Inquilino?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(Inquilino inquilino);
    Task AtualizarAsync(Inquilino inquilino);
    Task ExcluirAsync(int id);
}
