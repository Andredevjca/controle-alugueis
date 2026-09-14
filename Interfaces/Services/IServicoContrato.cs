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
    Task<ContratoListaViewModel> ObterListaAsync(string? busca, string? status, int pagina = 1);
    Task<ContratoViewModel> PrepararFormularioAsync(ContratoViewModel modelo);
    Task<ContratoViewModel?> ObterFormularioAsync(int id);
    Task<ContratoViewModel> NovoFormularioAsync(int? casaId, int? inquilinoId);
    Task<EncerrarContratoViewModel?> ObterEncerramentoAsync(int id);
    Task CancelarAsync(int id);
    Task EncerrarAsync(EncerrarContratoViewModel modelo);
}
