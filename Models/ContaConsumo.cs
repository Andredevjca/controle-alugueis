namespace SistemaAlugueis.Models;

public class ContaConsumo
{
    public int Id { get; set; }
    public int CasaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public decimal? LeituraAnterior { get; set; }
    public decimal? LeituraAtual { get; set; }
    public decimal? Consumo { get; set; }
    public decimal Valor { get; set; }
    public DateTime Vencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string Status { get; set; } = "Pendente";
    public string? Observacoes { get; set; }
    public DateTime DataCadastro { get; set; }
    public string? CasaNome { get; set; }
}
