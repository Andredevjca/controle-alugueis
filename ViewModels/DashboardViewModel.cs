using SistemaAlugueis.Models;

namespace SistemaAlugueis.ViewModels;

public class DashboardViewModel
{
    public int TotalCasas { get; set; }
    public int CasasAlugadas { get; set; }
    public int CasasDisponiveis { get; set; }
    public int CasasManutencao { get; set; }
    public decimal TotalReceberMes { get; set; }
    public decimal TotalRecebidoMes { get; set; }
    public decimal TotalPendenteMes { get; set; }
    public decimal TotalAtrasado { get; set; }
    public int QtdAlugueisAtrasados { get; set; }
    public decimal DespesasMes { get; set; }
    public decimal ReceitasMes { get; set; }
    public decimal SaldoMes { get; set; }
    public IEnumerable<CardCasaDashboardViewModel> Casas { get; set; } = [];
    public IEnumerable<VencimentoDashboardViewModel> ProximosVencimentos { get; set; } = [];
    public IEnumerable<Contrato> ContratosProximosVencimento { get; set; } = [];
}

public class CardCasaDashboardViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? InquilinoAtual { get; set; }
    public decimal ValorAluguel { get; set; }
    public int DiaVencimento { get; set; }
    public string SituacaoFinanceira { get; set; } = "Em dia";
}

public class VencimentoDashboardViewModel
{
    public string Casa { get; set; } = string.Empty;
    public string Inquilino { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime Vencimento { get; set; }
    public decimal Valor { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RelatorioViewModel
{
    public string TipoRelatorio { get; set; } = "casas";
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int? CasaId { get; set; }
    public int? InquilinoId { get; set; }
    public string? Status { get; set; }
    public string Titulo { get; set; } = "Relatórios";
    public IEnumerable<IDictionary<string, object?>> Linhas { get; set; } = [];
    public IEnumerable<string> Colunas { get; set; } = [];
    public decimal? Total { get; set; }
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Casas { get; set; } = [];
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Inquilinos { get; set; } = [];
}

public class LoginViewModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe o e-mail.")]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    [System.ComponentModel.DataAnnotations.Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe a senha.")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Display(Name = "Lembrar-me")]
    public bool Lembrar { get; set; }
}

public class UsuarioViewModel
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Display(Name = "Senha")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    public string? Senha { get; set; }

    [System.ComponentModel.DataAnnotations.Display(Name = "Perfil")]
    public string Perfil { get; set; } = "Administrador";

    public bool Ativo { get; set; } = true;
}

public class CategoriaViewModel
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public string Nome { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string Tipo { get; set; } = "Receita";

    public bool Ativo { get; set; } = true;
}

public class ConfiguracaoViewModel
{
    public IEnumerable<SistemaAlugueis.Models.Configuracao> Itens { get; set; } = [];
}
