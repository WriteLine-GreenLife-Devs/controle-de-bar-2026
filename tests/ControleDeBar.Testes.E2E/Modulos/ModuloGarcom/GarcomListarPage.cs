using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloGarcom;

public sealed class GarcomListarPage(
    IPage page,
    string urlBase
)
{
    public string Url => $"{urlBase}/Garcom/Listar";

    public ILocator Titulo => page.GetByRole(
        AriaRole.Heading,
        new() { Name = "Garçons", Exact = true }
    );

    public ILocator CadastrarGarcom => page.GetByRole(
        AriaRole.Link,
        new() { Name = "Cadastrar Garçom", Exact = true }
    );

    public ILocator EstadoVazio => page.GetByTestId("garcons-estado-vazio");

    public async Task IrParaAsync()
    {
        await page.GotoAsync(Url);
    }

    public ILocator LinhaPorNome(string nome)
    {
        return page
            .GetByTestId("garcom-linha")
            .Filter(new() { HasText = nome });
    }

    public async Task EditarAsync(string nome)
    {
        await LinhaPorNome(nome).GetByRole(
            AriaRole.Link,
            new() { Name = "Editar", Exact = true }
        ).ClickAsync();
    }

    public async Task ExcluirAsync(string nome)
    {
        await LinhaPorNome(nome).GetByRole(
            AriaRole.Link,
            new() { Name = "Excluir", Exact = true }
        ).ClickAsync();
    }
}
