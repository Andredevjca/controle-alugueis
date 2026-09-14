using SistemaAlugueis.Interfaces.Services;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.ViewModels;

namespace SistemaAlugueis.Services;

public class ServicoInquilino : IServicoInquilino
{
    private readonly IRepositorioInquilino _inquilino;
    private readonly IRepositorioContrato _contratos;
    private readonly IRepositorioCasa _casas;
    private readonly IRepositorioFinanceiro _financeiro;
    private readonly IRepositorioObservacao _observacoes;

    public ServicoInquilino(IRepositorioInquilino inquilino, IRepositorioContrato contratos, IRepositorioCasa casas, IRepositorioFinanceiro financeiro, IRepositorioObservacao observacoes)
    {
        _inquilino = inquilino;
        _contratos = contratos;
        _casas = casas;
        _financeiro = financeiro;
        _observacoes = observacoes;
    }

    public Task<(IEnumerable<Inquilino> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanho)
        => _inquilino.ListarAsync(busca, pagina, tamanho);

    public Task<IEnumerable<Inquilino>> ListarTodosAsync() => _inquilino.ListarTodosAsync();

    public Task<Inquilino?> ObterAsync(int id) => _inquilino.ObterPorIdAsync(id);

    public async Task<int> SalvarAsync(InquilinoViewModel modelo)
    {
        var entidade = Mapear(modelo);
        if (modelo.Id == 0)
        {
            return await _inquilino.InserirAsync(entidade);
        }

        await _inquilino.AtualizarAsync(entidade);
        return modelo.Id;
    }

    public async Task ExcluirAsync(int id)
    {
        var ativo = await _contratos.ObterAtivoPorInquilinoAsync(id);
        if (ativo != null)
        {
            throw new InvalidOperationException("Não é possível excluir um inquilino com contrato ativo.");
        }

        await _inquilino.ExcluirAsync(id);
    }

    public async Task<InquilinoDetalhesViewModel> ObterDetalhesAsync(int id)
    {
        var inquilino = await _inquilino.ObterPorIdAsync(id) ?? throw new InvalidOperationException("Inquilino não encontrado.");
        var historico = (await _contratos.ListarPorInquilinoAsync(id)).ToList();
        var atual = historico.FirstOrDefault(c => c.Status == StatusContrato.Ativo);
        Casa? casa = null;
        if (atual != null)
        {
            casa = await _casas.ObterPorIdAsync(atual.CasaId);
        }

        return new InquilinoDetalhesViewModel
        {
            Inquilino = inquilino,
            ContratoAtual = atual,
            CasaAtual = casa,
            TempoResidencia = atual == null ? "-" : Formatador.TempoResidencia(atual.DataInicio),
            HistoricoContratos = historico.Select(c => new HistoricoInquilinoViewModel
            {
                ContratoId = c.Id,
                InquilinoId = c.InquilinoId,
                NomeInquilino = c.CasaNome ?? "-",
                NumeroContrato = c.Numero,
                DataEntrada = c.DataInicio,
                DataSaida = c.DataFim,
                ValorAluguel = c.ValorAluguel,
                Status = c.Status,
                Atual = c.Status == StatusContrato.Ativo,
                TempoResidencia = Formatador.TempoResidencia(c.DataInicio, c.DataFim)
            }),
            HistoricoFinanceiro = await _financeiro.ListarPorInquilinoAsync(id),
            Observacoes = await _observacoes.ListarPorInquilinoAsync(id)
        };
    }

    public static InquilinoViewModel ParaFormulario(Inquilino i) => new()
    {
        Id = i.Id,
        NomeCompleto = i.NomeCompleto,
        Cpf = i.Cpf,
        Rg = i.Rg,
        DataNascimento = i.DataNascimento,
        Telefone = i.Telefone,
        Whatsapp = i.Whatsapp,
        Email = i.Email,
        Profissao = i.Profissao,
        Renda = i.Renda,
        EnderecoAnterior = i.EnderecoAnterior,
        Observacoes = i.Observacoes
    };

    private static Inquilino Mapear(InquilinoViewModel m) => new()
    {
        Id = m.Id,
        NomeCompleto = m.NomeCompleto,
        Cpf = m.Cpf,
        Rg = m.Rg,
        DataNascimento = m.DataNascimento,
        Telefone = m.Telefone,
        Whatsapp = m.Whatsapp,
        Email = m.Email,
        Profissao = m.Profissao,
        Renda = m.Renda,
        EnderecoAnterior = m.EnderecoAnterior,
        Observacoes = m.Observacoes
    };
}
