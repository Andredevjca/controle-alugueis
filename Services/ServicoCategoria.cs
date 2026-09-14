using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Models;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoCategoria : IServicoCategoria
{
    private readonly IRepositorioCategoria _repositorio;

    public ServicoCategoria(IRepositorioCategoria repositorio)
    {
        _repositorio = repositorio;
    }
    public Task<IEnumerable<CategoriaFinanceira>> ListarAsync() => _repositorio.ListarAsync();
    public Task<int> CriarAsync(CategoriaViewModel modelo) => _repositorio.InserirAsync(new CategoriaFinanceira { Nome = modelo.Nome, Tipo = modelo.Tipo, Ativo = true });
}
