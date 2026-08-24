using System.Globalization;
using System.Text.RegularExpressions;
using ControleDeBar.Testes.E2E.Compartilhado;
using ControleDeBar.Testes.E2E.Modulos.ModuloConta;
using ControleDeBar.Testes.E2E.Modulos.ModuloGarcom;
using ControleDeBar.Testes.E2E.Modulos.ModuloMesa;
using ControleDeBar.Testes.E2E.Modulos.ModuloPedido;
using ControleDeBar.Testes.E2E.Modulos.ModuloProduto;
using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloFaturamento;

[TestClass]
public sealed class FaturamentoE2ETests : E2ETestsBase
{
    private const string SenhaValida = "Senha123!";

    [TestMethod]
    public async Task Deve_ExibirZero_QuandoNaoHouverFaturamentoNaData()
    {
        await RegistrarEEntrarAsync("faturamento.vazio@teste.local", SenhaValida);
        FaturamentoPage faturamentoPage = new(Page, UrlBase);

        await Page.GetByRole(
            AriaRole.Link,
            new() { Name = "Faturamento", Exact = true }
        ).ClickAsync();
        await faturamentoPage.ConsultarDataAsync(DateTime.Today.AddDays(-1));

        await Expect(faturamentoPage.Total).ToContainTextAsync("R$ 0,00");
        await Expect(faturamentoPage.EstadoVazio).ToBeVisibleAsync();
        await Expect(faturamentoPage.EstadoVazio).ToContainTextAsync(
            "Nenhuma conta fechada nesta data."
        );
    }

    [TestMethod]
    public async Task Deve_ExibirContaFechada_EFaturamentoDoDia()
    {
        await RegistrarEEntrarAsync("faturamento.conta@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        Guid contaId = await PrepararContaAsync("Carlos", 1);
        await AdicionarPedidoAsync(contaId, "Cerveja", 8.50m, 2);
        await FecharContaAsync(contaId);

        FaturamentoPage faturamentoPage = new(Page, UrlBase);
        await faturamentoPage.IrParaAsync();
        await faturamentoPage.ConsultarDataAsync(DateTime.Today);

        ILocator linha = faturamentoPage.LinhasContas.Filter(new() { HasText = "Carlos" });
        await Expect(linha).ToBeVisibleAsync();
        await Expect(linha).ToContainTextAsync("Mesa 1");
        await Expect(linha).ToContainTextAsync(DateTime.Today.ToString("dd/MM/yyyy"));
        await Expect(linha).ToContainTextAsync("R$ 17,00");
        await Expect(faturamentoPage.Total).ToContainTextAsync("R$ 17,00");
    }

    [TestMethod]
    public async Task Deve_SomarVariasContasFechadasNoMesmoDia()
    {
        await RegistrarEEntrarAsync("faturamento.varias.contas@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        await CadastrarProdutoAsync("Refrigerante", 5.00m);
        Guid primeiraContaId = await PrepararContaAsync("Carlos", 1);
        await AdicionarPedidoAsync(primeiraContaId, "Cerveja", 8.50m, 2);
        await FecharContaAsync(primeiraContaId);

        Guid segundaContaId = await PrepararContaAsync("Maria", 1);
        await AdicionarPedidoAsync(segundaContaId, "Refrigerante", 5.00m, 1);
        await FecharContaAsync(segundaContaId);

        FaturamentoPage faturamentoPage = new(Page, UrlBase);
        await faturamentoPage.IrParaAsync();
        await faturamentoPage.ConsultarDataAsync(DateTime.Today);

        await Expect(faturamentoPage.LinhasContas).ToHaveCountAsync(2);
        await Expect(faturamentoPage.LinhasContas.Filter(new() { HasText = "Carlos" })).ToContainTextAsync("R$ 17,00");
        await Expect(faturamentoPage.LinhasContas.Filter(new() { HasText = "Maria" })).ToContainTextAsync("R$ 5,00");
        await Expect(faturamentoPage.Total).ToContainTextAsync("R$ 22,00");
    }

    [TestMethod]
    public async Task Deve_IgnorarContaAbertaNoFaturamento()
    {
        await RegistrarEEntrarAsync("faturamento.conta.aberta@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        Guid contaId = await PrepararContaAsync("Carlos", 1);
        await AdicionarPedidoAsync(contaId, "Cerveja", 8.50m, 2);

        FaturamentoPage faturamentoPage = new(Page, UrlBase);
        await faturamentoPage.IrParaAsync();
        await faturamentoPage.ConsultarDataAsync(DateTime.Today);

        await Expect(faturamentoPage.LinhasContas).ToHaveCountAsync(0);
        await Expect(faturamentoPage.Total).ToContainTextAsync("R$ 0,00");
        await Expect(faturamentoPage.EstadoVazio).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_ConsultarDataInformada_EManterDataNoFormulario()
    {
        await RegistrarEEntrarAsync("faturamento.data@teste.local", SenhaValida);
        FaturamentoPage faturamentoPage = new(Page, UrlBase);
        DateTime dataConsultada = new(2026, 8, 23);

        await faturamentoPage.IrParaAsync();
        await faturamentoPage.ConsultarDataAsync(dataConsultada);

        await Expect(faturamentoPage.Data).ToHaveValueAsync("2026-08-23");
        StringAssert.Contains(Page.Url, "Data=2026-08-23");
        await Expect(faturamentoPage.EstadoVazio).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_ConsiderarContaFechadaSemPedidosComTotalZero()
    {
        await RegistrarEEntrarAsync("faturamento.sem.pedidos@teste.local", SenhaValida);
        Guid contaId = await PrepararContaAsync("Carlos", 1);
        await FecharContaAsync(contaId);

        FaturamentoPage faturamentoPage = new(Page, UrlBase);
        await faturamentoPage.IrParaAsync();
        await faturamentoPage.ConsultarDataAsync(DateTime.Today);

        ILocator linha = faturamentoPage.LinhasContas.Filter(new() { HasText = "Carlos" });
        await Expect(linha).ToContainTextAsync("R$ 0,00");
        await Expect(faturamentoPage.Total).ToContainTextAsync("R$ 0,00");
    }

    [TestMethod]
    public async Task Deve_UsarSnapshotDoPedidoNoFaturamento()
    {
        await RegistrarEEntrarAsync("faturamento.snapshot@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Coca-Cola", 8.00m);
        Guid contaId = await PrepararContaAsync("Carlos", 1);
        await AdicionarPedidoAsync(contaId, "Coca-Cola", 8.00m, 2);

        ProdutoListarPage produtoListarPage = new(Page, UrlBase);
        ProdutoFormPage produtoFormPage = new(Page, UrlBase);
        await produtoListarPage.IrParaAsync();
        await produtoListarPage.EditarAsync("Coca-Cola");
        await produtoFormPage.PreencherAsync(
            "Coca-Cola 600ml",
            12.00m.ToString("F2", CultureInfo.InvariantCulture)
        );
        await produtoFormPage.ConfirmarAsync();

        await Page.GotoAsync(UrlDetalhes(contaId));
        await FecharContaAsync(contaId);

        FaturamentoPage faturamentoPage = new(Page, UrlBase);
        await faturamentoPage.IrParaAsync();
        await faturamentoPage.ConsultarDataAsync(DateTime.Today);

        ILocator linha = faturamentoPage.LinhasContas.Filter(new() { HasText = "Carlos" });
        await Expect(linha).ToContainTextAsync("R$ 16,00");
        await Expect(linha).Not.ToContainTextAsync("R$ 24,00");
        await Expect(faturamentoPage.Total).ToContainTextAsync("R$ 16,00");
    }

    private async Task CadastrarProdutoAsync(string nome, decimal preco)
    {
        ProdutoFormPage formPage = new(Page, UrlBase);
        ProdutoListarPage listarPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(nome, preco.ToString("F2", CultureInfo.InvariantCulture));
        await formPage.ConfirmarAsync();
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
    }

    private async Task<Guid> PrepararContaAsync(string nomeCliente, int numeroMesa)
    {
        MesaFormPage mesaFormPage = new(Page, UrlBase);
        await mesaFormPage.IrParaCadastroAsync();
        await mesaFormPage.PreencherAsync(numeroMesa, 4);
        await mesaFormPage.ConfirmarAsync();

        GarcomFormPage garcomFormPage = new(Page, UrlBase);
        await garcomFormPage.IrParaCadastroAsync();
        await garcomFormPage.PreencherAsync($"Marcos {numeroMesa}");
        await garcomFormPage.ConfirmarAsync();

        ContaFormPage contaFormPage = new(Page, UrlBase);
        ContaListarPage contaListarPage = new(Page, UrlBase);
        await contaFormPage.IrParaCadastroAsync();
        await contaFormPage.PreencherAsync(nomeCliente, numeroMesa, 4, $"Marcos {numeroMesa}");
        await contaFormPage.ConfirmarAsync();
        await contaListarPage.AbrirDetalhesAsync(nomeCliente);

        return ExtrairIdDaUrl(Page.Url);
    }

    private async Task AdicionarPedidoAsync(Guid contaId, string nomeProduto, decimal preco, int quantidade)
    {
        PedidoFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaAdicionarAsync(contaId);
        await formPage.SelecionarProdutoAsync(nomeProduto, preco);
        await formPage.Quantidade.FillAsync(quantidade.ToString());
        await formPage.ConfirmarAsync();
        await Expect(Page).ToHaveURLAsync(UrlDetalhes(contaId));
    }

    private async Task FecharContaAsync(Guid contaId)
    {
        await Page.GetByRole(
            AriaRole.Link,
            new() { Name = "Fechar Conta", Exact = true }
        ).ClickAsync();
        await Page.GetByRole(
            AriaRole.Button,
            new() { Name = "Confirmar Fechamento", Exact = true }
        ).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@".*/Conta/Listar$"));
        await Page.GotoAsync(UrlDetalhes(contaId));
    }

    private string UrlDetalhes(Guid contaId) => $"{UrlBase}/Conta/Detalhes/{contaId}";

    private static Guid ExtrairIdDaUrl(string url)
    {
        return Guid.Parse(new Uri(url).Segments[^1]);
    }
}
