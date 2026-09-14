using SistemaAlugueis.Models;

namespace SistemaAlugueis.Interfaces.Repositories;

public interface IRepositorioContrato
{
    Task<(IEnumerable<Contrato> Itens, int Total)> ListarAsync(string? busca, string? status, int pagina, int tamanho);
    Task<IEnumerable<Contrato>> ListarTodosAsync();
    Task<Contrato?> ObterPorIdAsync(int id);
    Task<Contrato?> ObterAtivoPorCasaAsync(int casaId);
    Task<Contrato?> ObterAtivoPorInquilinoAsync(int inquilinoId);
    Task<IEnumerable<Contrato>> ListarPorCasaAsync(int casaId);
    Task<IEnumerable<Contrato>> ListarPorInquilinoAsync(int inquilinoId);
    Task<IEnumerable<Contrato>> ListarProximosVencimentoAsync(int dias);
    Task<int> InserirAsync(Contrato contrato);
    Task AtualizarAsync(Contrato contrato);
    Task EncerrarAsync(int id, DateTime dataFim, string status);
}
