using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloProduto;

public sealed class ProdutoFormPage(
    IPage page,
    string urlBase
)
{
    public string UrlCadastrar => $"{urlBase}/Produto/Cadastrar";

    public ILocator Nome => page.GetByLabel("Nome");
    public ILocator Preco => page.GetByLabel("Preço");
    public ILocator Erros => page.Locator(".validation-summary-errors, .field-validation-error");

    public async Task IrParaCadastroAsync()
    {
        await page.GotoAsync(UrlCadastrar);
    }

    public async Task PreencherAsync(string nome, string preco)
    {
        await Nome.FillAsync(nome);
        await Preco.FillAsync(preco);
    }

    public async Task PreencherAsync(string nome, decimal preco)
    {
        await PreencherAsync(nome, preco.ToString("F2"));
    }

    public async Task ConfirmarAsync()
    {
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Confirmar", Exact = true }
        ).ClickAsync();
    }
}
