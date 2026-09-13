namespace SistemaAlugueis.Models;

public class Contrato
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int CasaId { get; set; }
    public int InquilinoId { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataTermino { get; set; }
    public DateTime? DataFim { get; set; }
    public decimal ValorAluguel { get; set; }
    public int DiaVencimento { get; set; }
    public decimal ValorCaucao { get; set; }
    public int MesesCaucao { get; set; }
    public string? IndiceReajuste { get; set; }
    public decimal PercentualMulta { get; set; }
    public decimal PercentualJuros { get; set; }
    public string Status { get; set; } = "Ativo";
    public string? Observacoes { get; set; }
    public DateTime DataCadastro { get; set; }
    public string? CasaNome { get; set; }
    public string? InquilinoNome { get; set; }
}
