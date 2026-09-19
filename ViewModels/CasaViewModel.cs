using System.ComponentModel.DataAnnotations;
using SistemaAlugueis.Helpers;

namespace SistemaAlugueis.ViewModels;

public class CasaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome da casa é obrigatório.")]
    [Display(Name = "Nome da casa")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "CEP")]
    public string? Cep { get; set; }

    [Required(ErrorMessage = "O endereço é obrigatório.")]
    [Display(Name = "Endereço")]
    public string Endereco { get; set; } = string.Empty;

    [Display(Name = "Número")]
    public string? Numero { get; set; }

    [Display(Name = "Complemento")]
    public string? Complemento { get; set; }

    [Display(Name = "Bairro")]
    public string? Bairro { get; set; }

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [Display(Name = "Cidade")]
    public string Cidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório.")]
    [Display(Name = "Estado")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Informe a UF com 2 letras.")]
    public string Estado { get; set; } = string.Empty;

    [Required(ErrorMessage = "O valor do aluguel é obrigatório.")]
    [Range(0.01, 9999999, ErrorMessage = "O valor do aluguel deve ser maior que zero.")]
    [Display(Name = "Valor do aluguel")]
    public decimal ValorAluguel { get; set; }

    [Range(1, 31, ErrorMessage = "O dia do vencimento deve estar entre 1 e 31.")]
    [Display(Name = "Dia do vencimento")]
    public int DiaVencimento { get; set; } = 10;

    [Display(Name = "Área em m²")]
    public decimal? AreaM2 { get; set; }

    [Display(Name = "Quantidade de quartos")]
    public int QtdQuartos { get; set; }

    [Display(Name = "Quantidade de banheiros")]
    public int QtdBanheiros { get; set; }

    [Display(Name = "Quantidade de vagas")]
    public int QtdVagas { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; } = StatusCasa.Disponivel;

    [Display(Name = "Número do medidor de água")]
    [StringLength(80, ErrorMessage = "Informe no máximo 80 caracteres.")]
    public string? NumeroMedidorAgua { get; set; }

    [Display(Name = "Número do medidor de luz")]
    [StringLength(80, ErrorMessage = "Informe no máximo 80 caracteres.")]
    public string? NumeroMedidorLuz { get; set; }

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }
}

public class CasaListaViewModel
{
    public IEnumerable<CasaListaItemViewModel> Itens { get; set; } = [];
    public string? Busca { get; set; }
    public string? Status { get; set; }
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
}

public class CasaListaItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string EnderecoCompleto { get; set; } = string.Empty;
    public decimal ValorAluguel { get; set; }
    public string? InquilinoAtual { get; set; }
    public string Status { get; set; } = string.Empty;
}
