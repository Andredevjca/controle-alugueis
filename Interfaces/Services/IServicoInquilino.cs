using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoInquilino
{
    Task<(IEnumerable<Inquilino> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanho);
    Task<IEnumerable<Inquilino>> ListarTodosAsync();
    Task<Inquilino?> ObterAsync(int id);
    Task<int> SalvarAsync(InquilinoViewModel modelo);
    Task ExcluirAsync(int id);
    Task<InquilinoDetalhesViewModel> ObterDetalhesAsync(int id);
}
