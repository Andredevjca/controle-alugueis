using System.Data;

namespace SistemaAlugueis.Database.Migrations;

public interface IMigration
{
    string Id { get; }
    Task AplicarAsync(IDbConnection db);
}
