# Sistema de Controle de Aluguéis

Aplicação web em **ASP.NET Core MVC** para administração de imóveis alugados, com Razor, Bootstrap 5, Bootstrap Icons, Dapper e MySQL 8+.

## Requisitos

- .NET 10 SDK
- MySQL 8 ou superior

## Banco de dados

1. Ajuste usuário e senha em `appsettings.json` (`ConnectionStrings:MySql`).
2. Execute o script `Database/schema.sql` no MySQL. Ele cria o banco `controle_alugueis`, as tabelas, categorias e dados de exemplo (5 casas, inquilinos, contratos, financeiro e contas de consumo).

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
