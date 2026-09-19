using Dapper;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.Repositories;

public class RepositorioCasa : IRepositorioCasa
{
    private readonly ConexaoBanco _conexao;

    public RepositorioCasa(ConexaoBanco conexao)
    {
        _conexao = conexao;
    }

    private const string Colunas = @"
        c.id AS Id, c.nome AS Nome, c.cep AS Cep, c.endereco AS Endereco, c.numero AS Numero,
        c.complemento AS Complemento, c.bairro AS Bairro, c.cidade AS Cidade, c.estado AS Estado,
        c.valor_aluguel AS ValorAluguel, c.dia_vencimento AS DiaVencimento, c.area_m2 AS AreaM2,
        c.qtd_quartos AS QtdQuartos, c.qtd_banheiros AS QtdBanheiros, c.qtd_vagas AS QtdVagas,
        c.numero_medidor_agua AS NumeroMedidorAgua,
        c.numero_medidor_luz AS NumeroMedidorLuz,
        c.status AS Status, c.observacoes AS Observacoes, c.ativo AS Ativo, c.data_cadastro AS DataCadastro,
        i.nome_completo AS InquilinoAtual, ct.id AS ContratoAtualId";

    public async Task<(IEnumerable<Casa> Itens, int Total)> ListarAsync(string? busca, string? status, int pagina, int tamanho)
    {
        using var db = _conexao.Criar();
        var filtro = "WHERE c.ativo = 1";
        if (!string.IsNullOrWhiteSpace(busca))
        {
            filtro += " AND (c.nome LIKE @Busca OR c.endereco LIKE @Busca OR c.bairro LIKE @Busca OR c.cidade LIKE @Busca)";
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filtro += " AND c.status = @Status";
        }

        var sqlTotal = $"SELECT COUNT(*) FROM casas c {filtro}";
        var total = await db.ExecuteScalarAsync<int>(sqlTotal, new { Busca = $"%{busca}%", Status = status });
        var offset = (pagina - 1) * tamanho;
        var sql = $@"
            SELECT {Colunas}
            FROM casas c
            LEFT JOIN contratos ct ON ct.casa_id = c.id AND ct.status = '{StatusContrato.Ativo}'
            LEFT JOIN inquilinos i ON i.id = ct.inquilino_id
            {filtro}
            ORDER BY c.nome
            LIMIT @Tamanho OFFSET @Offset";
        var itens = await db.QueryAsync<Casa>(sql, new { Busca = $"%{busca}%", Status = status, Tamanho = tamanho, Offset = offset });
        return (itens, total);
    }

    public async Task<IEnumerable<Casa>> ListarTodasAsync()
    {
        using var db = _conexao.Criar();
        return await db.QueryAsync<Casa>($@"
            SELECT {Colunas}
            FROM casas c
            LEFT JOIN contratos ct ON ct.casa_id = c.id AND ct.status = '{StatusContrato.Ativo}'
            LEFT JOIN inquilinos i ON i.id = ct.inquilino_id
            WHERE c.ativo = 1
            ORDER BY c.nome");
    }

    public async Task<Casa?> ObterPorIdAsync(int id)
    {
        using var db = _conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<Casa>($@"
            SELECT {Colunas}
            FROM casas c
            LEFT JOIN contratos ct ON ct.casa_id = c.id AND ct.status = '{StatusContrato.Ativo}'
            LEFT JOIN inquilinos i ON i.id = ct.inquilino_id
            WHERE c.id = @Id", new { Id = id });
    }

    public async Task<int> InserirAsync(Casa casa)
    {
        using var db = _conexao.Criar();
        return await db.ExecuteScalarAsync<int>(@"
            INSERT INTO casas (nome, cep, endereco, numero, complemento, bairro, cidade, estado, valor_aluguel,
                dia_vencimento, area_m2, qtd_quartos, qtd_banheiros, qtd_vagas, status, numero_medidor_agua, numero_medidor_luz, observacoes, ativo)
            VALUES (@Nome, @Cep, @Endereco, @Numero, @Complemento, @Bairro, @Cidade, @Estado, @ValorAluguel,
                @DiaVencimento, @AreaM2, @QtdQuartos, @QtdBanheiros, @QtdVagas, @Status, @NumeroMedidorAgua, @NumeroMedidorLuz, @Observacoes, 1);
            SELECT LAST_INSERT_ID();", casa);
    }

    public async Task AtualizarAsync(Casa casa)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE casas SET nome=@Nome, cep=@Cep, endereco=@Endereco, numero=@Numero, complemento=@Complemento,
                bairro=@Bairro, cidade=@Cidade, estado=@Estado, valor_aluguel=@ValorAluguel, dia_vencimento=@DiaVencimento,
                area_m2=@AreaM2, qtd_quartos=@QtdQuartos, qtd_banheiros=@QtdBanheiros, qtd_vagas=@QtdVagas,
                status=@Status, numero_medidor_agua=@NumeroMedidorAgua, numero_medidor_luz=@NumeroMedidorLuz, observacoes=@Observacoes
            WHERE id=@Id", casa);
    }

    public async Task ExcluirAsync(int id)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync("UPDATE casas SET ativo = 0 WHERE id = @Id", new { Id = id });
    }

    public async Task AtualizarStatusAsync(int id, string status)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync("UPDATE casas SET status = @Status WHERE id = @Id", new { Id = id, Status = status });
    }

    public async Task<int> ContarAsync(string? status = null)
    {
        using var db = _conexao.Criar();
        if (string.IsNullOrWhiteSpace(status))
        {
            return await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM casas WHERE ativo = 1");
        }

        return await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM casas WHERE ativo = 1 AND status = @Status", new { Status = status });
    }
}
