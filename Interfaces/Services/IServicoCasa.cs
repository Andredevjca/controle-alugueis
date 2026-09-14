using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoCasa
{
    Task<(IEnumerable<Casa> Itens, int Total)> ListarAsync(string? busca, string? status, int pagina, int tamanho);
    Task<IEnumerable<Casa>> ListarTodasAsync();
    Task<Casa?> ObterAsync(int id);
    Task<int> SalvarAsync(CasaViewModel modelo);
    Task ExcluirAsync(int id);
    Task<CasaDetalhesViewModel> ObterDetalhesAsync(int id, string aba = "resumo");
}
