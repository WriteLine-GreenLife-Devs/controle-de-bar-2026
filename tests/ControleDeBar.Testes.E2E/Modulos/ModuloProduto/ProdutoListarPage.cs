using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloProduto;

public sealed class ProdutoListarPage(
    IPage page,
    string urlBase
)
{
    public string Url => $"{urlBase}/Produto/Listar";

    public ILocator Titulo => page.GetByRole(
        AriaRole.Heading,
        new() { Name = "Produtos", Exact = true }
    );

    public ILocator CadastrarProduto => page.GetByRole(
        AriaRole.Link,
        new() { Name = "Cadastrar Produto", Exact = true }
    );

    public ILocator EstadoVazio => page.GetByTestId("produtos-estado-vazio");

    public ILocator CampoBusca => page.GetByPlaceholder("Buscar por nome...");

    public ILocator BotaoBusca => page.GetByRole(
        AriaRole.Button,
        new() { Name = "Buscar", Exact = true }
    );

    public async Task IrParaAsync()
    {
        await page.GotoAsync(Url);
    }

    public ILocator LinhaProduto()
    {
        return page.GetByTestId("produto-linha");
    }

    public ILocator LinhasProdutos()
    {
        return LinhaProduto();
    }

    public async Task EditarAsync(string nomeProduto)
    {
        ILocator linha = page
            .GetByTestId("produto-linha")
            .Filter(new() { HasText = nomeProduto });

        await linha.GetByRole(
            AriaRole.Link,
            new() { Name = "Editar", Exact = true }
        ).ClickAsync();
    }

    public async Task ExcluirAsync(string nomeProduto)
    {
        ILocator linha = page
            .GetByTestId("produto-linha")
            .Filter(new() { HasText = nomeProduto });

        await linha.GetByRole(
            AriaRole.Link,
            new() { Name = "Excluir", Exact = true }
        ).ClickAsync();
    }

    public async Task BuscarAsync(string nome)
    {
        await CampoBusca.FillAsync(nome);
        await BotaoBusca.ClickAsync();
    }

    public async Task LimparBuscaAsync()
    {
        await CampoBusca.FillAsync(string.Empty);
        await BotaoBusca.ClickAsync();
    }
}
