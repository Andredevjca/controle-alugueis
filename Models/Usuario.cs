namespace SistemaAlugueis.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Perfil { get; set; } = "Administrador";
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; }
}

public class CategoriaFinanceira
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}

public class Configuracao
{
    public int Id { get; set; }
    public string Chave { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public string? Descricao { get; set; }
}
