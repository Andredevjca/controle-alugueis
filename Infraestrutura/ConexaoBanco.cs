using System.Data;
using MySqlConnector;

namespace SistemaAlugueis.Infraestrutura;

public class ConexaoBanco
{
    private readonly string _stringConexao;

    public ConexaoBanco(IConfiguration configuracao)
    {
        _stringConexao = configuracao.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("String de conexão MySql não configurada.");
    }

    public IDbConnection Criar()
    {
        return new MySqlConnection(_stringConexao);
    }
}
