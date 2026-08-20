using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloAutenticacao;

public sealed class EntrarPage(
    IPage page,
    string urlBase
)
{
    public string Url => $"{urlBase}/Autenticacao/Entrar";

    public ILocator Titulo => page.GetByRole(
        AriaRole.Heading,
        new() { Name = "Entrar", Exact = true }
    );

    public ILocator Email => page.GetByLabel("E-mail");
    public ILocator Senha => page.GetByLabel("Senha", new() { Exact = true });
    public ILocator Erros => page.Locator(".validation-summary-errors");

    public ILocator UsuarioAutenticado(string email) => page.GetByRole(
        AriaRole.Button,
        new() { Name = email, Exact = true }
    );

    public async Task IrParaAsync()
    {
        await page.GotoAsync(Url);
    }

    public async Task PreencherAsync(string email, string senha)
    {
        await Email.FillAsync(email);
        await Senha.FillAsync(senha);
    }

    public async Task ConfirmarAsync()
    {
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Entrar", Exact = true }
        ).ClickAsync();
    }

    public async Task SairAsync(string email)
    {
        await UsuarioAutenticado(email).ClickAsync();

        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Sair", Exact = true }
        ).ClickAsync();
    }
}
