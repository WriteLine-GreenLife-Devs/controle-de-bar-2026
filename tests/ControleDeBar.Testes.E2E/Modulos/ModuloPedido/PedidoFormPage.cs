using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloPedido;

public sealed class PedidoFormPage(
    IPage page,
    string urlBase
)
{
    public string UrlAdicionar(Guid contaId) => $"{urlBase}/Pedido/Adicionar?contaId={contaId}";

    public ILocator Produto => page.GetByLabel("Produto");
    public ILocator Quantidade => page.GetByLabel("Quantidade");
    public ILocator Erros => page.Locator(".validation-summary-errors, .field-validation-error");

    public async Task IrParaAdicionarAsync(Guid contaId)
    {
        await page.GotoAsync(UrlAdicionar(contaId));
    }

    public async Task SelecionarProdutoAsync(string nome, decimal preco)
    {
        await Produto.SelectOptionAsync(new SelectOptionValue
        {
            Label = $"{nome} - {preco:C}"
        });
    }

    public async Task ConfirmarAsync()
    {
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Adicionar", Exact = true }
        ).ClickAsync();
    }
}
