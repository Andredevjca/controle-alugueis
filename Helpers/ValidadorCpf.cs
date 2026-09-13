namespace SistemaAlugueis.Helpers;

public static class ValidadorCpf
{
    public static bool EhValido(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            return true;
        }

        var numeros = new string(cpf.Where(char.IsDigit).ToArray());
        if (numeros.Length != 11)
        {
            return false;
        }

        if (numeros.Distinct().Count() == 1)
        {
            return false;
        }

        var soma = 0;
        for (var i = 0; i < 9; i++)
        {
            soma += (numeros[i] - '0') * (10 - i);
        }

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;
        if (digito1 != numeros[9] - '0')
        {
            return false;
        }

        soma = 0;
        for (var i = 0; i < 10; i++)
        {
            soma += (numeros[i] - '0') * (11 - i);
        }

        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;
        return digito2 == numeros[10] - '0';
    }
}
