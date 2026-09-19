using Dapper;
using MySqlConnector;
using System.Data;

namespace SistemaAlugueis.Database.Migrations;

public sealed class IdentificacaoAguaLuz : IMigration
{
    public string Id => "20260919_IdentificacaoAguaLuz";

    public async Task AplicarAsync(IDbConnection db)
    {

        // Nomes definidos pela aplicação, nunca recebidos do usuário.
        (string Tabela, string Coluna)[] colunas =
        [
            ("casas", "numero_medidor_agua"),
            ("casas", "numero_medidor_luz"),
            ("inquilinos", "identificacao_conta_agua"),
            ("inquilinos", "identificacao_conta_luz")
        ];

        foreach (var (tabela, coluna) in colunas)
        {
            var existe = await db.ExecuteScalarAsync<int>("""
                SELECT COUNT(*) FROM information_schema.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = @Tabela AND COLUMN_NAME = @Coluna
                """, new { Tabela = tabela, Coluna = coluna });

            if (existe > 0)
                continue;

            try
            {
                await db.ExecuteAsync(
                    $"ALTER TABLE `{tabela}` ADD COLUMN `{coluna}` VARCHAR(80) NULL");
            }
            catch (MySqlException ex) when (ex.Number == 1060)
            {
                // Outra instância pode ter adicionado o campo após a verificação.
            }
        }
    }
}
