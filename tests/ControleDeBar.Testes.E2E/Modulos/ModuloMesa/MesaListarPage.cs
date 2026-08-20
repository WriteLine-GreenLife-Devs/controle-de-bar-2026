using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloMesa;

public sealed class MesaListarPage(
    IPage page,
    string urlBase
)
{
    public string Url => $"{urlBase}/Mesa/Listar";

    public ILocator Titulo => page.GetByRole(
        AriaRole.Heading,
        new() { Name = "Mesas", Exact = true }
    );

    public ILocator CadastrarMesa => page.GetByRole(
        AriaRole.Link,
        new() { Name = "Cadastrar Mesa", Exact = true }
    );

    public ILocator EstadoVazio => page.GetByTestId("mesas-estado-vazio");

    public async Task IrParaAsync()
    {
        await page.GotoAsync(Url);
    }

    public ILocator CardPorNumero(int numero)
    {
        return page
            .GetByTestId("mesa-card")
            .Filter(new() { HasText = $"Mesa {numero}" });
    }

    public ILocator StatusDaMesa(int numero)
    {
        return CardPorNumero(numero).GetByTestId("mesa-status");
    }

    public async Task EditarAsync(int numero)
    {
        await CardPorNumero(numero).GetByRole(
            AriaRole.Link,
            new() { Name = "Editar", Exact = true }
        ).ClickAsync();
    }

    public async Task ExcluirAsync(int numero)
    {
        await CardPorNumero(numero).GetByRole(
            AriaRole.Link,
            new() { Name = "Excluir", Exact = true }
        ).ClickAsync();
    }
}
