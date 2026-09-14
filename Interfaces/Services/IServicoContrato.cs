using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Interfaces.Services;

public interface IServicoContrato
{
    Task<(IEnumerable<Contrato> Itens, int Total)> ListarAsync(string? busca, string? status, int pagina, int tamanho);
    Task<IEnumerable<Contrato>> ListarTodosAsync();
    Task<Contrato?> ObterAsync(int id);
    Task<int> SalvarAsync(ContratoViewModel modelo);
    Task EncerrarAsync(int id, DateTime dataSaida, string status);
}
