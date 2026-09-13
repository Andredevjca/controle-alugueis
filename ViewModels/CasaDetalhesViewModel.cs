using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.ViewModels;

public class CasaDetalhesViewModel
{
    public Casa Casa { get; set; } = new();
    public Contrato? ContratoAtual { get; set; }
    public Inquilino? InquilinoAtual { get; set; }
    public string TempoResidencia { get; set; } = "-";
    public DateTime? ProximoVencimento { get; set; }
    public string SituacaoFinanceira { get; set; } = "Em dia";
    public IEnumerable<LancamentoFinanceiro> Lancamentos { get; set; } = [];
    public IEnumerable<ContaConsumo> ContasAgua { get; set; } = [];
    public IEnumerable<ContaConsumo> ContasLuz { get; set; } = [];
    public IEnumerable<Observacao> Observacoes { get; set; } = [];
    public IEnumerable<HistoricoInquilinoViewModel> Historico { get; set; } = [];
    public decimal MediaConsumoAgua { get; set; }
    public decimal MediaValorAgua { get; set; }
    public decimal MediaConsumoLuz { get; set; }
    public decimal MediaValorLuz { get; set; }
    public ContaConsumo? UltimaAgua { get; set; }
    public ContaConsumo? UltimaLuz { get; set; }
    public string Aba { get; set; } = "resumo";
}

public class HistoricoInquilinoViewModel
{
    public int ContratoId { get; set; }
    public int InquilinoId { get; set; }
    public string NomeInquilino { get; set; } = string.Empty;
    public string NumeroContrato { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public DateTime? DataSaida { get; set; }
    public decimal ValorAluguel { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Atual { get; set; }
    public string TempoResidencia { get; set; } = string.Empty;
}
