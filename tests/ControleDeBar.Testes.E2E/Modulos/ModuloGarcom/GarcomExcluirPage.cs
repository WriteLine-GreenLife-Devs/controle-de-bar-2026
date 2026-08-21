using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloGarcom;

public sealed class GarcomExcluirPage(IPage page)
{
    public ILocator MensagemConfirmacao => page.GetByText(
        "Deseja realmente excluir este garçom?",
        new() { Exact = true }
    );

    public async Task ConfirmarAsync()
    {
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Confirmar", Exact = true }
        ).ClickAsync();
    }
}
