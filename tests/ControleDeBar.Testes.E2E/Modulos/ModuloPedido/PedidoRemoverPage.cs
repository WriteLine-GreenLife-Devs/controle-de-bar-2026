using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloPedido;

public sealed class PedidoRemoverPage(IPage page)
{
    public ILocator Produto(string nome) => page.GetByText(nome, new() { Exact = true });
    public ILocator Preco(string preco) => page.GetByText(preco, new() { Exact = true });
    public ILocator Quantidade(string quantidade) => page.GetByText(quantidade, new() { Exact = true });
    public ILocator Subtotal(string subtotal) => page.GetByText(subtotal, new() { Exact = true });
    public ILocator Confirmacao => page.GetByText(
        "Deseja realmente remover este pedido?",
        new() { Exact = true }
    );

    public async Task ConfirmarAsync()
    {
        await page.GetByRole(
            AriaRole.Button,
            new() { Name = "Confirmar Remoção", Exact = true }
        ).ClickAsync();
    }
}
