namespace SistemaAlugueis.Models;

public class Observacao
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int? CasaId { get; set; }
    public int? ContratoId { get; set; }
    public int? InquilinoId { get; set; }
    public int? LancamentoId { get; set; }
    public int? ContaConsumoId { get; set; }
    public DateTime DataObservacao { get; set; }
    public int? UsuarioId { get; set; }
    public string? CasaNome { get; set; }
    public string? InquilinoNome { get; set; }
    public string? UsuarioNome { get; set; }
}
