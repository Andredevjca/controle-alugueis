# Sistema de Controle de Aluguéis

Aplicação web em **ASP.NET Core MVC** para administração de imóveis alugados, com Razor, Bootstrap 5, Bootstrap Icons, Dapper e MySQL 8+.

## Requisitos

- .NET 10 SDK
- MySQL 8 ou superior

## Banco de dados

1. Ajuste usuário e senha em `appsettings.json` (`ConnectionStrings:MySql`).
2. Execute o script `Database/schema.sql` no MySQL. Ele cria o banco `controle_alugueis`, as tabelas, categorias e dados de exemplo (5 casas, inquilinos, contratos, financeiro e contas de consumo).

Os campos de identificação das contas de água/luz e dos medidores são adicionados automaticamente ao banco configurado na inicialização, antes de atender requisições. A atualização verifica cada coluna e adiciona somente as ausentes, preservando os registros e permitindo reinicializações ou retomada após uma atualização parcial. As alterações ficam em `Database/migrations`, são executadas automaticamente pelo `AtualizadorBanco` e registradas na tabela `schema_migrations` somente após sucesso. Migrations já aplicadas são ignoradas nas próximas inicializações. O usuário do MySQL configurado na aplicação precisa de permissões `CREATE`, `ALTER`, `SELECT` e `INSERT` para essa atualização. Não é necessário executar scripts manualmente. Não execute `schema.sql` em um banco existente, pois ele recria as tabelas.

Cadastre as identificações das contas em **Inquilinos > Editar** e os medidores em **Casas > Editar**. Os campos são opcionais e preservam letras, zeros iniciais e pontuação. Consulte os dados em **Casas > Detalhes > Água/Luz** ou nos detalhes do inquilino. A consulta de pendências no fornecedor é manual.

O usuário administrador é criado automaticamente na primeira execução, se a tabela `usuarios` estiver vazia.

- E-mail: `admin@sistema.com`
- Senha: `Admin@123`

## Execução

```bash
dotnet restore
dotnet run
```

Acesse `https://localhost:5xxx` (porta indicada no terminal) e entre com o usuário administrador.

## Módulos

- Dashboard com cards, situação das casas, vencimentos e resumo financeiro
- Casas (cadastro, detalhes, abas, histórico em timeline)
- Inquilinos
- Contratos (encerramento preserva histórico)
- Financeiro (receitas, despesas, pagamentos)
- Contas de água e luz
- Observações
- Relatórios
- Categorias, usuários e configurações

A exclusão de casas e inquilinos é lógica (inativação). Contratos não são apagados: o status passa para **Encerrado** e a data de saída é gravada.

Valores aparecem em real (`R$ 1.200,00`) e datas no padrão brasileiro (`dd/MM/yyyy`).
