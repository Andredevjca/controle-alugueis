using System.Globalization;

namespace SistemaAlugueis.Helpers;

public static class Formatador
{
    private static readonly CultureInfo Cultura = new("pt-BR");

    public static string Moeda(decimal? valor)
    {
        return (valor ?? 0).ToString("C", Cultura);
    }

    public static string Data(DateTime? data)
    {
        return data?.ToString("dd/MM/yyyy", Cultura) ?? "-";
    }

    public static string DataHora(DateTime? data)
    {
        return data?.ToString("dd/MM/yyyy HH:mm", Cultura) ?? "-";
    }

    public static string TempoResidencia(DateTime entrada, DateTime? saida = null)
    {
        var fim = saida ?? DateTime.Today;
        if (fim < entrada)
        {
            return "0 dias";
        }

        var anos = fim.Year - entrada.Year;
        var meses = fim.Month - entrada.Month;
        var dias = fim.Day - entrada.Day;

        if (dias < 0)
        {
            meses--;
            var mesAnterior = fim.AddMonths(-1);
            dias += DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month);
        }

        if (meses < 0)
        {
            anos--;
            meses += 12;
        }

        var partes = new List<string>();
        if (anos > 0)
        {
            partes.Add(anos == 1 ? "1 ano" : $"{anos} anos");
        }

        if (meses > 0)
        {
            partes.Add(meses == 1 ? "1 mês" : $"{meses} meses");
        }

        partes.Add(dias == 1 ? "1 dia" : $"{dias} dias");

        if (partes.Count == 1)
        {
            return partes[0];
        }

        if (partes.Count == 2)
        {
            return $"{partes[0]} e {partes[1]}";
        }

        return $"{partes[0]}, {partes[1]} e {partes[2]}";
    }

    public static string ClasseStatus(string? status)
    {
        return status switch
        {
            StatusFinanceiro.Pago or StatusCasa.Alugada or "Em dia" or StatusContrato.Ativo => "status-pago",
            StatusFinanceiro.Pendente => "status-pendente",
            StatusFinanceiro.Atrasado or StatusContrato.Vencido => "status-atrasado",
            StatusFinanceiro.Cancelado or StatusContrato.Cancelado or StatusContrato.Encerrado => "status-cancelado",
            StatusCasa.Disponivel => "status-info",
            StatusCasa.Manutencao => "status-pendente",
            _ => "status-info"
        };
    }
}
