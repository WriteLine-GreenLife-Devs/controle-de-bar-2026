using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloAutenticacao;

public sealed class RegistrarPage(
    IPage page,
    string urlBase
)
{
    public string Url => $"{urlBase}/Autenticacao/Registrar";

    public ILocator Email => page.GetByLabel("E-mail");
    public ILocator Senha => page.GetByLabel("Senha", new() { Exact = true });
    public ILocator ConfirmarSenha => page.GetByLabel("Confirmar Senha");
    public ILocator Erros => page.Locator(".validation-summary-errors, .field-validation-error");

    public async Task IrParaAsync()
    {
        await page.GotoAsync(Url);
    }

    public async Task PreencherAsync(
        string email,
        string senha,
        string? confirmacaoSenha = null
    )
    {
        await Email.FillAsync(email);
        await Senha.FillAsync(senha);
        await ConfirmarSenha.FillAsync(confirmacaoSenha ?? senha);
    }

    public async Task ConfirmarAsync()
    {
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Criar Conta", Exact = true }
        ).ClickAsync();
    }
}
