# Controle de Bar

Sistema web multi-tenant para gerenciar mesas, garçons, produtos, contas e pedidos de um bar.

Cada dono de bar possui uma conta de usuário e administra somente os dados do próprio estabelecimento. Todas as entidades do domínio são vinculadas ao usuário autenticado, impedindo a consulta ou alteração de registros pertencentes a outro bar.

## Funcionalidades

### Autenticação

- Registrar um dono de bar com e-mail e senha.
- Autenticar o usuário para acessar o sistema.
- Encerrar a sessão do usuário.
- Isolar os dados pelo identificador do usuário autenticado.

### Mesas

- Cadastrar, editar, excluir e visualizar mesas.
- Informar o número e a quantidade de lugares.
- Manter o número da mesa único por usuário.
- Indicar se a mesa está livre ou ocupada.
- Permitir várias contas abertas simultaneamente na mesma mesa.
- Impedir a exclusão de mesas que possuam contas vinculadas.

### Garçons

- Cadastrar, editar, excluir e visualizar garçons.
- Manter o nome do garçom único por usuário.
- Impedir a exclusão de garçons que possuam contas vinculadas.

### Produtos

- Cadastrar, editar, excluir e visualizar produtos.
- Informar o nome e o preço de venda.
- Manter o nome do produto único por usuário.
- Impedir a exclusão de produtos que possuam pedidos vinculados.

### Contas e pedidos

- Abrir uma conta para um cliente, vinculando uma mesa e um garçom.
- Registrar a data de abertura e a data de fechamento.
- Adicionar e remover pedidos enquanto a conta estiver aberta.
- Registrar o produto, a quantidade e o preço praticado em cada pedido.
- Calcular o subtotal dos pedidos e o valor total da conta.
- Impedir alterações nos pedidos de uma conta fechada.
- Manter a mesa ocupada enquanto existir ao menos uma conta aberta.
- Liberar a mesa após o fechamento da última conta aberta.

### Faturamento

- Calcular o faturamento diário pela soma das contas fechadas na data consultada.
- Considerar a data de fechamento da conta.
- Restringir o faturamento ao usuário autenticado.

## Regras de negócio

1. Todo registro deve pertencer ao usuário autenticado.
2. Um usuário não pode acessar registros pertencentes a outro usuário.
3. O número da mesa deve ser único por usuário.
4. A quantidade de lugares da mesa deve ser maior que zero.
5. O nome do garçom deve ser obrigatório e único por usuário.
6. O nome do produto deve ser obrigatório e único por usuário.
7. O preço do produto deve ser maior que zero.
8. Mesas, garçons e produtos com vínculos não podem ser excluídos.
9. Uma mesa pode possuir várias contas abertas simultaneamente.
10. A mesa permanece ocupada enquanto possuir ao menos uma conta aberta.
11. A mesa fica livre somente quando não possuir contas abertas.
12. A mesa e o garçom da conta devem pertencer ao usuário autenticado.
13. Uma conta fechada não pode ser reaberta.
14. Uma conta fechada não aceita inclusão ou remoção de pedidos.
15. A quantidade do pedido deve ser maior que zero.
16. O produto do pedido deve pertencer ao usuário autenticado.
17. O pedido deve preservar o nome e o preço do produto no momento da inclusão.
18. O subtotal do pedido corresponde ao preço praticado multiplicado pela quantidade.
19. O total da conta corresponde à soma dos subtotais dos pedidos.
20. O faturamento diário considera as contas fechadas pela data de fechamento.

## Tecnologias

- C# e .NET 10;
- ASP.NET Core MVC;
- Entity Framework Core;
- SQL Server;
- ASP.NET Core Identity;
- AutoMapper;
- FluentResults;
- Serilog;
- MSTest e Moq;
- Playwright;
- GitHub Actions.

## Arquitetura

A solução utiliza projetos separados para Domínio, Aplicação, Infraestrutura e Apresentação.

```text
ControleDeBar.slnx
|-- src/
|   |-- ControleDeBar.Dominio/
|   |-- ControleDeBar.Aplicacao/
|   |-- ControleDeBar.Infra/
|   `-- ControleDeBar.WebApp/
`-- tests/
    |-- ControleDeBar.Testes.Unidade/
    |-- ControleDeBar.Testes.Integracao/
    `-- ControleDeBar.Testes.E2E/
```

### ControleDeBar.Dominio

Contém entidades, interfaces de repositório, validações e contratos compartilhados. Não referencia os demais projetos.

### ControleDeBar.Aplicacao

Contém serviços, DTOs e casos de uso. Referencia somente o projeto de Domínio.

### ControleDeBar.Infra

Contém o `DbContext`, configurações do Entity Framework Core, migrations, repositórios, Identity e logging. Referencia somente o projeto de Domínio.

### ControleDeBar.WebApp

Contém controllers, ViewModels, profiles do AutoMapper e Razor Views. Referencia os projetos de Aplicação e Infraestrutura e realiza a composição das dependências.

## Organização dos módulos

Cada funcionalidade é organizada no projeto correspondente:

```text
ControleDeBar.Dominio/Modulos/ModuloMesa/
ControleDeBar.Aplicacao/Modulos/ModuloMesa/
ControleDeBar.Infra/Modulos/ModuloMesa/
ControleDeBar.WebApp/Modulos/ModuloMesa/
```

O mesmo padrão deve ser aplicado aos módulos de Garçom, Produto, Conta e Pedido.

## Persistência e multi-tenancy

O `ControleDeBarDbContext` herda de `IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>`.

As entidades pertencentes a um usuário implementam `IEntidadeDoUsuario` e armazenam `UserId`. O contexto atribui o identificador do usuário ao cadastrar registros e impede alterações ou exclusões de entidades pertencentes a outro usuário.

Cada entidade deve receber um filtro global por `UserId` em `OnModelCreating`. Os mapeamentos do Entity Framework Core são carregados da assembly de Infraestrutura por `ApplyConfigurationsFromAssembly`.

As alterações no modelo de dados são versionadas em:

```text
src/ControleDeBar.Infra/Compartilhado/Orm/Migrations/
```

Em ambiente de desenvolvimento, as migrations pendentes são aplicadas na inicialização da aplicação.

## Testes

- Testes de domínio validam entidades sem mocks.
- Testes de aplicação validam serviços com Moq.
- Testes de integração validam repositórios com Entity Framework Core em ambiente isolado.
- Testes E2E validam os principais fluxos com Playwright.

## Execução

Configure a conexão `SqlServerEF` em `appsettings.Development.json` e execute:

```powershell
dotnet restore
dotnet build ControleDeBar.slnx
dotnet run --project src/ControleDeBar.WebApp
```
