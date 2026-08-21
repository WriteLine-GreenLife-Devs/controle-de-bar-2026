using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloGarcom;

public sealed class GarcomFormPage(
    IPage page,
    string urlBase
)
{
    public string UrlCadastrar => $"{urlBase}/Garcom/Cadastrar";

    public ILocator Nome => page.GetByLabel("Nome");
    public ILocator Erros => page.Locator(".validation-summary-errors, .field-validation-error");

    public async Task IrParaCadastroAsync()
    {
        await page.GotoAsync(UrlCadastrar);
    }

    public async Task PreencherAsync(string nome)
    {
        await Nome.FillAsync(nome);
    }

    public async Task ConfirmarAsync()
    {
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Confirmar", Exact = true }
        ).ClickAsync();
    }
}
