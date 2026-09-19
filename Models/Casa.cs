namespace SistemaAlugueis.Models;

public class Casa
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cep { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal ValorAluguel { get; set; }
    public int DiaVencimento { get; set; }
    public decimal? AreaM2 { get; set; }
    public int QtdQuartos { get; set; }
    public int QtdBanheiros { get; set; }
    public int QtdVagas { get; set; }
    public string Status { get; set; } = "Disponível";
    public string? NumeroMedidorAgua { get; set; }
    public string? NumeroMedidorLuz { get; set; }
    public string? Observacoes { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; }
    public string? InquilinoAtual { get; set; }
    public int? ContratoAtualId { get; set; }
}
