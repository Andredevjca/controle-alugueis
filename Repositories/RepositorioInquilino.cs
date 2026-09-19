using Dapper;
using SistemaAlugueis.Helpers;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Models;

namespace SistemaAlugueis.Repositories;

public class RepositorioInquilino : IRepositorioInquilino
{
    private readonly ConexaoBanco _conexao;

    public RepositorioInquilino(ConexaoBanco conexao)
    {
        _conexao = conexao;
    }

    private const string Colunas = @"
        i.id AS Id, i.nome_completo AS NomeCompleto, i.cpf AS Cpf, i.rg AS Rg,
        i.data_nascimento AS DataNascimento, i.telefone AS Telefone, i.whatsapp AS Whatsapp,
        i.email AS Email, i.profissao AS Profissao, i.renda AS Renda, i.endereco_anterior AS EnderecoAnterior,
        i.identificacao_conta_agua AS IdentificacaoContaAgua,
        i.identificacao_conta_luz AS IdentificacaoContaLuz,
        i.observacoes AS Observacoes, i.ativo AS Ativo, i.data_cadastro AS DataCadastro,
        ca.nome AS CasaAtual,
        CASE WHEN ct.id IS NULL THEN 'Disponível' ELSE 'Alugando' END AS Situacao";

    public async Task<(IEnumerable<Inquilino> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanho)
    {
        using var db = _conexao.Criar();
        var filtro = "WHERE i.ativo = 1";
        if (!string.IsNullOrWhiteSpace(busca))
        {
            filtro += " AND (i.nome_completo LIKE @Busca OR i.cpf LIKE @Busca OR i.email LIKE @Busca OR i.telefone LIKE @Busca)";
        }

        var total = await db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM inquilinos i {filtro}", new { Busca = $"%{busca}%" });
        var offset = (pagina - 1) * tamanho;
        var itens = await db.QueryAsync<Inquilino>($@"
            SELECT {Colunas}
            FROM inquilinos i
            LEFT JOIN contratos ct ON ct.inquilino_id = i.id AND ct.status = '{StatusContrato.Ativo}'
            LEFT JOIN casas ca ON ca.id = ct.casa_id
            {filtro}
            ORDER BY i.nome_completo
            LIMIT @Tamanho OFFSET @Offset", new { Busca = $"%{busca}%", Tamanho = tamanho, Offset = offset });
        return (itens, total);
    }

    public async Task<IEnumerable<Inquilino>> ListarTodosAsync()
    {
        using var db = _conexao.Criar();
        return await db.QueryAsync<Inquilino>($@"
            SELECT {Colunas}
            FROM inquilinos i
            LEFT JOIN contratos ct ON ct.inquilino_id = i.id AND ct.status = '{StatusContrato.Ativo}'
            LEFT JOIN casas ca ON ca.id = ct.casa_id
            WHERE i.ativo = 1
            ORDER BY i.nome_completo");
    }

    public async Task<Inquilino?> ObterPorIdAsync(int id)
    {
        using var db = _conexao.Criar();
        return await db.QueryFirstOrDefaultAsync<Inquilino>($@"
            SELECT {Colunas}
            FROM inquilinos i
            LEFT JOIN contratos ct ON ct.inquilino_id = i.id AND ct.status = '{StatusContrato.Ativo}'
            LEFT JOIN casas ca ON ca.id = ct.casa_id
            WHERE i.id = @Id", new { Id = id });
    }

    public async Task<int> InserirAsync(Inquilino inquilino)
    {
        using var db = _conexao.Criar();
        return await db.ExecuteScalarAsync<int>(@"
            INSERT INTO inquilinos (nome_completo, cpf, rg, data_nascimento, telefone, whatsapp, email, profissao, renda, endereco_anterior, identificacao_conta_agua, identificacao_conta_luz, observacoes, ativo)
            VALUES (@NomeCompleto, @Cpf, @Rg, @DataNascimento, @Telefone, @Whatsapp, @Email, @Profissao, @Renda, @EnderecoAnterior, @IdentificacaoContaAgua, @IdentificacaoContaLuz, @Observacoes, 1);
            SELECT LAST_INSERT_ID();", inquilino);
    }

    public async Task AtualizarAsync(Inquilino inquilino)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync(@"
            UPDATE inquilinos SET nome_completo=@NomeCompleto, cpf=@Cpf, rg=@Rg, data_nascimento=@DataNascimento,
                telefone=@Telefone, whatsapp=@Whatsapp, email=@Email, profissao=@Profissao, renda=@Renda,
                endereco_anterior=@EnderecoAnterior, identificacao_conta_agua=@IdentificacaoContaAgua, identificacao_conta_luz=@IdentificacaoContaLuz, observacoes=@Observacoes
            WHERE id=@Id", inquilino);
    }

    public async Task ExcluirAsync(int id)
    {
        using var db = _conexao.Criar();
        await db.ExecuteAsync("UPDATE inquilinos SET ativo = 0 WHERE id = @Id", new { Id = id });
    }
}
