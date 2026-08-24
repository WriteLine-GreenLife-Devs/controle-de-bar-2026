using System.Globalization;
using System.Text.RegularExpressions;
using ControleDeBar.Testes.E2E.Compartilhado;
using ControleDeBar.Testes.E2E.Modulos.ModuloAutenticacao;
using ControleDeBar.Testes.E2E.Modulos.ModuloConta;
using ControleDeBar.Testes.E2E.Modulos.ModuloGarcom;
using ControleDeBar.Testes.E2E.Modulos.ModuloMesa;
using ControleDeBar.Testes.E2E.Modulos.ModuloProduto;
using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloPedido;

[TestClass]
public sealed class PedidoE2ETests : E2ETestsBase
{
    private const string SenhaValida = "Senha123!";

    [TestMethod]
    public async Task Deve_ExibirEstadoInicial_QuandoContaNaoTemPedidos()
    {
        await RegistrarEEntrarAsync("pedido.vazio@teste.local", SenhaValida);
        Guid contaId = await PrepararContaAsync("Carlos");

        await Expect(Page.GetByTestId("pedidos-estado-vazio")).ToBeVisibleAsync();
        await Expect(TotalDaConta(0m)).ToBeVisibleAsync();
        await Expect(Page).ToHaveURLAsync(UrlDetalhes(contaId));
    }

    [TestMethod]
    public async Task Deve_AdicionarPedido_ComDadosValidos()
    {
        await RegistrarEEntrarAsync("pedido.adicionar@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        Guid contaId = await PrepararContaAsync("Carlos");

        await AdicionarPedidoAsync(contaId, "Cerveja", 8.50m, 2);

        ILocator linha = LinhaDoPedido("Cerveja");
        await Expect(linha).ToContainTextAsync("Cerveja");
        await Expect(linha).ToContainTextAsync("R$ 8,50");
        await Expect(linha).ToContainTextAsync("2");
        await Expect(linha).ToContainTextAsync("R$ 17,00");
        await Expect(TotalDaConta(17.00m)).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_ExibirMultiplosPedidos_ECalcularTotal()
    {
        await RegistrarEEntrarAsync("pedido.multiplos@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        await CadastrarProdutoAsync("Refrigerante", 5.00m);
        Guid contaId = await PrepararContaAsync("Carlos");

        await AdicionarPedidoAsync(contaId, "Cerveja", 8.50m, 2);
        await AdicionarPedidoAsync(contaId, "Refrigerante", 5.00m, 1);

        await Expect(Page.GetByTestId("pedido-linha")).ToHaveCountAsync(2);
        await Expect(LinhaDoPedido("Cerveja")).ToContainTextAsync("R$ 17,00");
        await Expect(LinhaDoPedido("Refrigerante")).ToContainTextAsync("R$ 5,00");
        await Expect(TotalDaConta(22.00m)).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_RejeitarAdicao_QuandoProdutoNaoSelecionado()
    {
        await RegistrarEEntrarAsync("pedido.produto.obrigatorio@teste.local", SenhaValida);
        Guid contaId = await PrepararContaAsync("Carlos");
        PedidoFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaAdicionarAsync(contaId);

        await formPage.Produto.SelectOptionAsync(string.Empty);
        await formPage.ConfirmarAsync();

        await Expect(Page).ToHaveURLAsync($"{UrlBase}/Pedido/Adicionar");
        await Expect(formPage.Erros).ToContainTextAsync("O campo \"Produto\" é obrigatório.");
    }

    [TestMethod]
    public async Task Deve_RejeitarAdicao_QuandoQuantidadeForZero()
    {
        await RegistrarEEntrarAsync("pedido.quantidade.zero@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        Guid contaId = await PrepararContaAsync("Carlos");
        PedidoFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaAdicionarAsync(contaId);
        await formPage.SelecionarProdutoAsync("Cerveja", 8.50m);
        await formPage.Quantidade.FillAsync("0");

        await formPage.ConfirmarAsync();

        await Expect(Page).ToHaveURLAsync(formPage.UrlAdicionar(contaId));
        Assert.IsFalse(await formPage.Quantidade.EvaluateAsync<bool>("element => element.validity.valid"));
        Assert.IsTrue(await formPage.Quantidade.EvaluateAsync<bool>("element => element.validity.rangeUnderflow"));
    }

    [TestMethod]
    public async Task Deve_RemoverPedido_UnicoEAtualizarTotal()
    {
        await RegistrarEEntrarAsync("pedido.remover.unico@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        Guid contaId = await PrepararContaAsync("Carlos");
        await AdicionarPedidoAsync(contaId, "Cerveja", 8.50m, 2);

        await RemoverPedidoAsync("Cerveja", "R$ 8,50", "2", "R$ 17,00");

        await Expect(Page).ToHaveURLAsync(UrlDetalhes(contaId));
        await Expect(Page.GetByTestId("pedidos-estado-vazio")).ToBeVisibleAsync();
        await Expect(TotalDaConta(0m)).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_RemoverApenasUmPedido_EManterOutro()
    {
        await RegistrarEEntrarAsync("pedido.remover.um@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        await CadastrarProdutoAsync("Refrigerante", 5.00m);
        Guid contaId = await PrepararContaAsync("Carlos");
        await AdicionarPedidoAsync(contaId, "Cerveja", 8.50m, 2);
        await AdicionarPedidoAsync(contaId, "Refrigerante", 5.00m, 1);

        await RemoverPedidoAsync("Cerveja", "R$ 8,50", "2", "R$ 17,00");

        await Expect(Page.GetByTestId("pedido-linha")).ToHaveCountAsync(1);
        await Expect(LinhaDoPedido("Refrigerante")).ToBeVisibleAsync();
        await Expect(TotalDaConta(5.00m)).ToBeVisibleAsync();
        await Expect(Page).ToHaveURLAsync(UrlDetalhes(contaId));
    }

    [TestMethod]
    public async Task Deve_PreservarSnapshotVisual_AposEdicaoDoProduto()
    {
        await RegistrarEEntrarAsync("pedido.snapshot.visual@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Coca-Cola", 8.00m);
        Guid contaId = await PrepararContaAsync("Carlos");
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
        ILocator linha = LinhaDoPedido("Coca-Cola");
        await Expect(linha).ToContainTextAsync("Coca-Cola");
        await Expect(linha).ToContainTextAsync("R$ 8,00");
        await Expect(linha).ToContainTextAsync("R$ 16,00");
        await Expect(linha).Not.ToContainTextAsync("Coca-Cola 600ml");
        await Expect(TotalDaConta(16.00m)).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_ManterPedidoVisivel_QuandoContaForFechada()
    {
        await RegistrarEEntrarAsync("pedido.conta.fechada@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        Guid contaId = await PrepararContaAsync("Carlos");
        await AdicionarPedidoAsync(contaId, "Cerveja", 8.50m, 2);
        await FecharContaAsync(contaId);

        await Page.GotoAsync(UrlDetalhes(contaId));
        await Expect(Page.GetByText("Fechada", new() { Exact = true })).ToBeVisibleAsync();
        ILocator linha = LinhaDoPedido("Cerveja");
        await Expect(linha).ToContainTextAsync("R$ 8,50");
        await Expect(linha).ToContainTextAsync("2");
        await Expect(linha).ToContainTextAsync("R$ 17,00");
        await Expect(TotalDaConta(17.00m)).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Adicionar Produto", Exact = true })).Not.ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Remover", Exact = true })).Not.ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_RejeitarAdicaoDireta_EmContaFechada()
    {
        await RegistrarEEntrarAsync("pedido.adicao.fechada@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        Guid contaId = await PrepararContaAsync("Carlos");
        await FecharContaAsync(contaId);

        PedidoFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaAdicionarAsync(contaId);
        await formPage.SelecionarProdutoAsync("Cerveja", 8.50m);
        await formPage.Quantidade.FillAsync("1");
        await formPage.ConfirmarAsync();

        await Expect(Page).ToHaveURLAsync($"{UrlBase}/Pedido/Adicionar");
        await Expect(formPage.Erros).ToContainTextAsync("Não é possível adicionar pedidos a uma conta fechada.");
    }

    [TestMethod]
    public async Task Deve_RejeitarRemocaoDireta_EmContaFechada()
    {
        await RegistrarEEntrarAsync("pedido.remocao.fechada@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        Guid contaId = await PrepararContaAsync("Carlos");
        await AdicionarPedidoAsync(contaId, "Cerveja", 8.50m, 2);

        string? urlRemover = await LinhaDoPedido("Cerveja")
            .GetByRole(AriaRole.Link, new() { Name = "Remover", Exact = true })
            .GetAttributeAsync("href");
        Assert.IsNotNull(urlRemover);

        await FecharContaAsync(contaId);
        await Page.GotoAsync($"{UrlBase}{urlRemover}");
        PedidoRemoverPage removerPage = new(Page);
        await removerPage.ConfirmarAsync();

        await Expect(Page).ToHaveURLAsync(UrlDetalhes(contaId));
        await Expect(LinhaDoPedido("Cerveja")).ToContainTextAsync("R$ 17,00");
    }

    [TestMethod]
    public async Task Deve_RejeitarExclusaoDeProdutoVinculadoAUmPedido()
    {
        await RegistrarEEntrarAsync("pedido.produto.vinculado@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Cerveja", 8.50m);
        Guid contaId = await PrepararContaAsync("Carlos");
        await AdicionarPedidoAsync(contaId, "Cerveja", 8.50m, 1);

        ProdutoListarPage listarPage = new(Page, UrlBase);
        ProdutoExcluirPage excluirPage = new(Page);
        await listarPage.IrParaAsync();
        await listarPage.ExcluirAsync("Cerveja");
        await excluirPage.ConfirmarAsync();

        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(Page.GetByText(
            "Não é possível excluir este produto, pois ele está vinculado a um pedido.",
            new() { Exact = true }
        )).ToBeVisibleAsync();
        await Expect(listarPage.LinhasProdutos()).ToContainTextAsync("Cerveja");
        await Expect(Page).Not.ToHaveURLAsync(new Regex($".*/Conta/Detalhes/{contaId}"));
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

    private async Task<Guid> PrepararContaAsync(string nomeCliente)
    {
        MesaFormPage mesaFormPage = new(Page, UrlBase);
        await mesaFormPage.IrParaCadastroAsync();
        await mesaFormPage.PreencherAsync(1, 4);
        await mesaFormPage.ConfirmarAsync();

        GarcomFormPage garcomFormPage = new(Page, UrlBase);
        await garcomFormPage.IrParaCadastroAsync();
        await garcomFormPage.PreencherAsync("Marcos");
        await garcomFormPage.ConfirmarAsync();

        ContaFormPage contaFormPage = new(Page, UrlBase);
        ContaListarPage contaListarPage = new(Page, UrlBase);
        await contaFormPage.IrParaCadastroAsync();
        await contaFormPage.PreencherAsync(nomeCliente, 1, 4, "Marcos");
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

    private async Task RemoverPedidoAsync(string nomeProduto, string preco, string quantidade, string subtotal)
    {
        ILocator linha = LinhaDoPedido(nomeProduto);
        await linha.GetByRole(
            AriaRole.Link,
            new() { Name = "Remover", Exact = true }
        ).ClickAsync();

        PedidoRemoverPage removerPage = new(Page);
        await Expect(removerPage.Confirmacao).ToBeVisibleAsync();
        await Expect(removerPage.Produto(nomeProduto)).ToBeVisibleAsync();
        await Expect(removerPage.Preco(preco)).ToBeVisibleAsync();
        await Expect(removerPage.Quantidade(quantidade)).ToBeVisibleAsync();
        await Expect(removerPage.Subtotal(subtotal)).ToBeVisibleAsync();
        await removerPage.ConfirmarAsync();
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

    private ILocator LinhaDoPedido(string nomeProduto)
    {
        return Page.GetByTestId("pedido-linha").Filter(new() { HasText = nomeProduto });
    }

    private ILocator TotalDaConta(decimal total)
    {
        return Page.GetByText($"Total: {total:C}", new() { Exact = true });
    }

    private string UrlDetalhes(Guid contaId) => $"{UrlBase}/Conta/Detalhes/{contaId}";

    private static Guid ExtrairIdDaUrl(string url)
    {
        return Guid.Parse(new Uri(url).Segments[^1]);
    }
}
