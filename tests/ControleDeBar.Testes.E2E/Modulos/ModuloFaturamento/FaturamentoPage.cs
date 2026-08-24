using System.Globalization;
using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloFaturamento;

public sealed class FaturamentoPage(
    IPage page,
    string urlBase
)
{
    public string Url => $"{urlBase}/Faturamento/Consultar";

    public ILocator Data => page.GetByLabel("Data");
    public ILocator Consultar => page.GetByRole(
        AriaRole.Button,
        new() { Name = "Consultar", Exact = true }
    );
    public ILocator Total => page.GetByTestId("faturamento-total");
    public ILocator EstadoVazio => page.GetByTestId("faturamento-estado-vazio");
    public ILocator LinhasContas => page.GetByTestId("faturamento-conta-linha");

    public async Task IrParaAsync()
    {
        await page.GotoAsync(Url);
    }

    public async Task ConsultarDataAsync(DateTime data)
    {
        await Data.FillAsync(data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        await Consultar.ClickAsync();
    }
}
