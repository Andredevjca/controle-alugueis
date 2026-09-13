namespace SistemaAlugueis.Models;

public class LancamentoFinanceiro
{
    public int Id { get; set; }
    public int? CasaId { get; set; }
    public int? ContratoId { get; set; }
    public int? InquilinoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int? CategoriaId { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataLancamento { get; set; }
    public DateTime Vencimento { get; set; }
    public decimal Valor { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string Status { get; set; } = "Pendente";
    public string? Observacoes { get; set; }
    public DateTime DataCadastro { get; set; }
    public string? CasaNome { get; set; }
    public string? CategoriaNome { get; set; }
    public string? InquilinoNome { get; set; }
}
