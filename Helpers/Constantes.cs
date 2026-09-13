namespace SistemaAlugueis.Helpers;

public static class StatusCasa
{
    public const string Disponivel = "Disponível";
    public const string Alugada = "Alugada";
    public const string Manutencao = "Manutenção";
}

public static class StatusContrato
{
    public const string Ativo = "Ativo";
    public const string Encerrado = "Encerrado";
    public const string Cancelado = "Cancelado";
    public const string Vencido = "Vencido";
}

public static class StatusFinanceiro
{
    public const string Pendente = "Pendente";
    public const string Pago = "Pago";
    public const string Atrasado = "Atrasado";
    public const string Cancelado = "Cancelado";
}

public static class TipoLancamento
{
    public const string Receita = "Receita";
    public const string Despesa = "Despesa";
}

public static class OrigemReceita
{
    public const string Aluguel = "Aluguel";
    public const string Caucao = "Caução";
    public const string Multa = "Multa";
    public const string Juros = "Juros";
    public const string Outros = "Outros";
}

public static class OrigemDespesa
{
    public const string Manutencao = "Manutenção";
    public const string Conserto = "Conserto";
    public const string Imposto = "Imposto";
    public const string Seguro = "Seguro";
    public const string Taxa = "Taxa";
    public const string Outros = "Outros";
}

public static class TipoConsumo
{
    public const string Agua = "Água";
    public const string Luz = "Luz";
}

public static class TipoObservacao
{
    public const string Casa = "Casa";
    public const string Contrato = "Contrato";
    public const string Inquilino = "Inquilino";
    public const string Financeiro = "Financeiro";
    public const string Agua = "Água";
    public const string Luz = "Luz";
    public const string Manutencao = "Manutenção";
}

public static class PerfilUsuario
{
    public const string Administrador = "Administrador";
    public const string Operador = "Operador";
}
