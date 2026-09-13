using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.ViewModels;

public class FinanceiroViewModel
{
    public int Id { get; set; }

    [Display(Name = "Casa")]
    public int? CasaId { get; set; }

    [Display(Name = "Contrato")]
    public int? ContratoId { get; set; }

    [Display(Name = "Inquilino")]
    public int? InquilinoId { get; set; }

    [Required]
    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = TipoLancamento.Receita;

    [Display(Name = "Categoria")]
    public int? CategoriaId { get; set; }

    [Required(ErrorMessage = "A origem é obrigatória.")]
    [Display(Name = "Origem")]
    public string Origem { get; set; } = OrigemReceita.Aluguel;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Data")]
    [DataType(DataType.Date)]
    public DateTime DataLancamento { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Vencimento")]
    [DataType(DataType.Date)]
    public DateTime Vencimento { get; set; } = DateTime.Today;

    [Range(0.01, 9999999, ErrorMessage = "O valor deve ser maior que zero.")]
    [Display(Name = "Valor")]
    public decimal Valor { get; set; }

    [Display(Name = "Data do pagamento")]
    [DataType(DataType.Date)]
    public DateTime? DataPagamento { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; } = StatusFinanceiro.Pendente;

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    public IEnumerable<SelectListItem> Casas { get; set; } = [];
    public IEnumerable<SelectListItem> Categorias { get; set; } = [];
    public IEnumerable<SelectListItem> Inquilinos { get; set; } = [];
}

public class FinanceiroListaViewModel
{
    public IEnumerable<LancamentoFinanceiro> Itens { get; set; } = [];
    public string? Tipo { get; set; }
    public string? Status { get; set; }
    public int? CasaId { get; set; }
    public int? CategoriaId { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Busca { get; set; }
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; }
    public IEnumerable<SelectListItem> Casas { get; set; } = [];
    public IEnumerable<SelectListItem> Categorias { get; set; } = [];
}

public class ContaConsumoViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A casa é obrigatória.")]
    [Display(Name = "Casa")]
    public int CasaId { get; set; }

    [Required]
    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = TipoConsumo.Agua;

    [Required(ErrorMessage = "A referência é obrigatória.")]
    [Display(Name = "Referência")]
    public string Referencia { get; set; } = DateTime.Today.ToString("yyyy-MM");

    [Display(Name = "Leitura anterior")]
    public decimal? LeituraAnterior { get; set; }

    [Display(Name = "Leitura atual")]
    public decimal? LeituraAtual { get; set; }

    [Display(Name = "Consumo")]
    public decimal? Consumo { get; set; }

    [Range(0.01, 9999999, ErrorMessage = "O valor deve ser maior que zero.")]
    [Display(Name = "Valor")]
    public decimal Valor { get; set; }

    [Required]
    [Display(Name = "Vencimento")]
    [DataType(DataType.Date)]
    public DateTime Vencimento { get; set; } = DateTime.Today.AddDays(10);

    [Display(Name = "Data do pagamento")]
    [DataType(DataType.Date)]
    public DateTime? DataPagamento { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; } = StatusFinanceiro.Pendente;

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    public IEnumerable<SelectListItem> Casas { get; set; } = [];
}

public class ContaConsumoListaViewModel
{
    public IEnumerable<ContaConsumo> Itens { get; set; } = [];
    public string? Tipo { get; set; }
    public int? CasaId { get; set; }
    public string? Status { get; set; }
    public string? Busca { get; set; }
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; }
    public IEnumerable<SelectListItem> Casas { get; set; } = [];
}

public class ObservacaoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O título é obrigatório.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = TipoObservacao.Casa;

    [Display(Name = "Casa")]
    public int? CasaId { get; set; }

    [Display(Name = "Contrato")]
    public int? ContratoId { get; set; }

    [Display(Name = "Inquilino")]
    public int? InquilinoId { get; set; }

    public IEnumerable<SelectListItem> Casas { get; set; } = [];
    public IEnumerable<SelectListItem> Contratos { get; set; } = [];
    public IEnumerable<SelectListItem> Inquilinos { get; set; } = [];
}

public class ObservacaoListaViewModel
{
    public IEnumerable<Observacao> Itens { get; set; } = [];
    public string? Tipo { get; set; }
    public int? CasaId { get; set; }
    public string? Busca { get; set; }
    public IEnumerable<SelectListItem> Casas { get; set; } = [];
}
