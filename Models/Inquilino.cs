namespace SistemaAlugueis.Models;

public class Inquilino
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Rg { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Telefone { get; set; }
    public string? Whatsapp { get; set; }
    public string? Email { get; set; }
    public string? Profissao { get; set; }
    public decimal? Renda { get; set; }
    public string? EnderecoAnterior { get; set; }
    public string? IdentificacaoContaAgua { get; set; }
    public string? IdentificacaoContaLuz { get; set; }
    public string? Observacoes { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; }
    public string? CasaAtual { get; set; }
    public string? Situacao { get; set; }
}
