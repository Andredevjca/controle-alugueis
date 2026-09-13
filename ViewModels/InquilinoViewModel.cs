using System.ComponentModel.DataAnnotations;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.ViewModels;

public class CpfValidoAttribute : ValidationAttribute
{
    public CpfValidoAttribute()
    {
        ErrorMessage = "Informe um CPF válido.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return ValidadorCpf.EhValido(value as string)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage);
    }
}

public class InquilinoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [CpfValido]
    [Display(Name = "CPF")]
    public string? Cpf { get; set; }

    [Display(Name = "RG")]
    public string? Rg { get; set; }

    [Display(Name = "Data de nascimento")]
    [DataType(DataType.Date)]
    public DateTime? DataNascimento { get; set; }

    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [Display(Name = "WhatsApp")]
    public string? Whatsapp { get; set; }

    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [Display(Name = "Profissão")]
    public string? Profissao { get; set; }

    [Display(Name = "Renda")]
    public decimal? Renda { get; set; }

    [Display(Name = "Endereço anterior")]
    public string? EnderecoAnterior { get; set; }

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }
}

public class InquilinoListaViewModel
{
    public IEnumerable<Inquilino> Itens { get; set; } = [];
    public string? Busca { get; set; }
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; }
}

public class InquilinoDetalhesViewModel
{
    public Inquilino Inquilino { get; set; } = new();
    public Contrato? ContratoAtual { get; set; }
    public Casa? CasaAtual { get; set; }
    public string TempoResidencia { get; set; } = "-";
    public IEnumerable<HistoricoInquilinoViewModel> HistoricoContratos { get; set; } = [];
    public IEnumerable<LancamentoFinanceiro> HistoricoFinanceiro { get; set; } = [];
    public IEnumerable<Observacao> Observacoes { get; set; } = [];
}
