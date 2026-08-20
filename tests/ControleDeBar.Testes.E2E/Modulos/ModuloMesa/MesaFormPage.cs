using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloMesa;

public sealed class MesaFormPage(
    IPage page,
    string urlBase
)
{
    public string UrlCadastrar => $"{urlBase}/Mesa/Cadastrar";

    public ILocator Numero => page.GetByLabel("Número");
    public ILocator Lugares => page.GetByLabel("Lugares");
    public ILocator Erros => page.Locator(".validation-summary-errors, .field-validation-error");

    public async Task IrParaCadastroAsync()
    {
        await page.GotoAsync(UrlCadastrar);
    }

    public async Task PreencherAsync(int numero, int lugares)
    {
        await Numero.FillAsync(numero.ToString());
        await Lugares.FillAsync(lugares.ToString());
    }

    public async Task ConfirmarAsync()
    {
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Confirmar", Exact = true }
        ).ClickAsync();
    }
}
