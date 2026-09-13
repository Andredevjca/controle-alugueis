using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.ViewModels;

public class ContratoViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A casa é obrigatória.")]
    [Display(Name = "Casa")]
    public int CasaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "O inquilino é obrigatório.")]
    [Display(Name = "Inquilino")]
    public int InquilinoId { get; set; }

    [Required(ErrorMessage = "O número do contrato é obrigatório.")]
    [Display(Name = "Número do contrato")]
    public string Numero { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data de início é obrigatória.")]
    [Display(Name = "Data de início")]
    [DataType(DataType.Date)]
    public DateTime DataInicio { get; set; } = DateTime.Today;

    [Display(Name = "Data de término")]
    [DataType(DataType.Date)]
    public DateTime? DataTermino { get; set; }

    [Range(0.01, 9999999, ErrorMessage = "O valor do aluguel deve ser maior que zero.")]
    [Display(Name = "Valor do aluguel")]
    public decimal ValorAluguel { get; set; }

    [Range(1, 31, ErrorMessage = "O dia do vencimento deve estar entre 1 e 31.")]
    [Display(Name = "Dia do vencimento")]
    public int DiaVencimento { get; set; } = 10;

    [Display(Name = "Valor da caução")]
    public decimal ValorCaucao { get; set; }

    [Display(Name = "Quantidade de meses de caução")]
    public int MesesCaucao { get; set; }

    [Display(Name = "Índice de reajuste")]
    public string? IndiceReajuste { get; set; }

    [Display(Name = "Percentual de multa")]
    public decimal PercentualMulta { get; set; } = 10;

    [Display(Name = "Percentual de juros")]
    public decimal PercentualJuros { get; set; } = 1;

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    public IEnumerable<SelectListItem> Casas { get; set; } = [];
    public IEnumerable<SelectListItem> Inquilinos { get; set; } = [];
}

public class ContratoListaViewModel
{
    public IEnumerable<Contrato> Itens { get; set; } = [];
    public string? Busca { get; set; }
    public string? Status { get; set; }
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; }
}

public class EncerrarContratoViewModel
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string CasaNome { get; set; } = string.Empty;
    public string InquilinoNome { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Data de saída")]
    [DataType(DataType.Date)]
    public DateTime DataSaida { get; set; } = DateTime.Today;
}
