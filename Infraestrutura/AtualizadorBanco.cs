using Dapper;
using SistemaAlugueis.Database.Migrations;

namespace SistemaAlugueis.Infraestrutura;

public class AtualizadorBanco(ConexaoBanco conexao, ILogger<AtualizadorBanco> logger)
{
    public async Task AtualizarAsync()
    {
        using var db = conexao.Criar();
        db.Open();

        // O bloqueio pertence à conexão e evita migrações simultâneas entre instâncias.
        var nomeBloqueio = await db.ExecuteScalarAsync<string>(
            "SELECT CONCAT('alugueis:migrations:', LEFT(SHA2(DATABASE(), 256), 40))");
        var bloqueado = await db.ExecuteScalarAsync<int?>(
            "SELECT GET_LOCK(@Nome, 60)", new { Nome = nomeBloqueio }, commandTimeout: 65);
        if (bloqueado != 1)
            throw new InvalidOperationException("Não foi possível obter o bloqueio para atualizar o banco.");

        try
        {
            await db.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS schema_migrations (
                    id VARCHAR(150) NOT NULL PRIMARY KEY,
                    aplicada_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                ) ENGINE=InnoDB
                """);

            // Adicione as próximas migrations nesta lista em ordem de execução.
            IMigration[] migrations = [new IdentificacaoAguaLuz()];
            var aplicadas = (await db.QueryAsync<string>("SELECT id FROM schema_migrations"))
                .ToHashSet(StringComparer.Ordinal);

            foreach (var migration in migrations)
            {
                if (aplicadas.Contains(migration.Id))
                    continue;

                logger.LogInformation("Aplicando migration {Migration}.", migration.Id);
                // DDL no MySQL faz commit implícito. Cada migration deve permitir retomada.
                await migration.AplicarAsync(db);
                await db.ExecuteAsync("INSERT INTO schema_migrations (id) VALUES (@Id)",
                    new { migration.Id });
                logger.LogInformation("Migration {Migration} aplicada.", migration.Id);
            }
        }
        finally
        {
            await db.ExecuteScalarAsync<int?>("SELECT RELEASE_LOCK(@Nome)", new { Nome = nomeBloqueio });
        }
    }
}
