using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloConta;

public sealed class ContaFormPage(
    IPage page,
    string urlBase
)
{
    public string UrlAbrir => $"{urlBase}/Conta/Abrir";

    public ILocator NomeCliente => page.GetByLabel("Nome do Cliente");
    public ILocator Mesa => page.GetByLabel("Mesa");
    public ILocator Garcom => page.GetByLabel("Garçom");
    public ILocator Erros => page.Locator(".validation-summary-errors, .field-validation-error");

    public async Task IrParaCadastroAsync()
    {
        await page.GotoAsync(UrlAbrir);
    }

    public async Task PreencherAsync(string nomeCliente, int numeroMesa, int lugaresMesa, string nomeGarcom)
    {
        await NomeCliente.FillAsync(nomeCliente);
        await Mesa.SelectOptionAsync(new[] { $"Mesa {numeroMesa} - {lugaresMesa} lugares" });
        await Garcom.SelectOptionAsync(new[] { nomeGarcom });
    }

    public async Task ConfirmarAsync()
    {
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Abrir Conta", Exact = true }
        ).ClickAsync();
    }
}
