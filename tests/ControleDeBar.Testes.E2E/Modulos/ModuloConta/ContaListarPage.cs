using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloConta;

public sealed class ContaListarPage(
    IPage page,
    string urlBase
)
{
    public string Url => $"{urlBase}/Conta/Listar";

    public ILocator Titulo => page.GetByRole(
        AriaRole.Heading,
        new() { Name = "Contas", Exact = true }
    );

    public ILocator AbrirConta => page.GetByRole(
        AriaRole.Link,
        new() { Name = "Abrir Conta", Exact = true }
    );

    public ILocator EstadoVazioAbertas => page.GetByTestId("contas-abertas-estado-vazio");
    public ILocator EstadoVazioFechadas => page.GetByTestId("contas-fechadas-estado-vazio");

    public ILocator LinhasAbertas => page.GetByTestId("conta-aberta-linha");
    public ILocator LinhasFechadas => page.GetByTestId("conta-fechada-linha");

    public ILocator LinhaAbertaPorCliente(string nomeCliente)
    {
        return LinhasAbertas.Filter(new() { HasText = nomeCliente });
    }

    public ILocator LinhaFechadaPorCliente(string nomeCliente)
    {
        return LinhasFechadas.Filter(new() { HasText = nomeCliente });
    }

    public async Task IrParaAsync()
    {
        await page.GotoAsync(Url);
    }

    public async Task AbrirDetalhesAsync(string nomeCliente)
    {
        await LinhaAbertaPorCliente(nomeCliente).GetByRole(
            AriaRole.Link,
            new() { Name = "Detalhes", Exact = true }
        ).ClickAsync();
    }

    public async Task FecharAsync(string nomeCliente)
    {
        await LinhaAbertaPorCliente(nomeCliente).GetByRole(
            AriaRole.Link,
            new() { Name = "Fechar", Exact = true }
        ).ClickAsync();
    }
}
