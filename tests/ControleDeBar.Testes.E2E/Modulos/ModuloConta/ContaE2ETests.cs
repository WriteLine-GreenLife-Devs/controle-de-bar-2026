using System.Text.RegularExpressions;
using ControleDeBar.Testes.E2E.Compartilhado;
using ControleDeBar.Testes.E2E.Modulos.ModuloAutenticacao;
using ControleDeBar.Testes.E2E.Modulos.ModuloGarcom;
using ControleDeBar.Testes.E2E.Modulos.ModuloMesa;
using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloConta;

[TestClass]
public sealed class ContaE2ETests : E2ETestsBase
{
    private const string SenhaValida = "Senha123!";

    [TestMethod]
    public async Task Deve_ExibirListagemVazia_ParaUsuarioSemContas()
    {
        // Arrange
        await RegistrarEEntrarAsync("conta.vazia@teste.local", SenhaValida);
        ContaListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.Titulo).ToBeVisibleAsync();
        await Expect(listarPage.AbrirConta).ToBeVisibleAsync();
        await Expect(listarPage.EstadoVazioAbertas).ToBeVisibleAsync();
        await Expect(listarPage.EstadoVazioFechadas).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_AbrirConta_ComDadosValidos()
    {
        // Arrange
        await RegistrarEEntrarAsync("conta.abertura@teste.local", SenhaValida);
        await CadastrarMesaAsync(1, 4);
        await CadastrarGarcomAsync("Marcos");

        ContaListarPage listarPage = new(Page, UrlBase);
        ContaFormPage formPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();
        await listarPage.AbrirConta.ClickAsync();
        await formPage.PreencherAsync("Carlos", 1, 4, "Marcos");
        await formPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.LinhaAbertaPorCliente("Carlos")).ToBeVisibleAsync();
        await Expect(listarPage.LinhaAbertaPorCliente("Carlos")).ToContainTextAsync("Mesa 1");
        await Expect(listarPage.LinhaAbertaPorCliente("Carlos")).ToContainTextAsync("Marcos");
        await Expect(listarPage.LinhaAbertaPorCliente("Carlos")).ToContainTextAsync("Carlos");
    }

    [TestMethod]
    public async Task Deve_RejeitarAbertura_QuandoNomeClienteVazio()
    {
        // Arrange
        await RegistrarEEntrarAsync("conta.nome.vazio@teste.local", SenhaValida);
        await CadastrarMesaAsync(1, 4);
        await CadastrarGarcomAsync("Marcos");

        ContaFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();

        // Act
        await formPage.Mesa.SelectOptionAsync(new[] { "Mesa 1 - 4 lugares" });
        await formPage.Garcom.SelectOptionAsync(new[] { "Marcos" });
        await formPage.NomeCliente.FillAsync(string.Empty);
        await formPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(formPage.UrlAbrir);
        await Expect(formPage.Erros).ToContainTextAsync("O campo \"Nome do cliente\" é obrigatório.");
    }

    [TestMethod]
    public async Task Deve_RejeitarAbertura_QuandoMesaNaoSelecionada()
    {
        // Arrange
        await RegistrarEEntrarAsync("conta.mesa.vazia@teste.local", SenhaValida);
        await CadastrarGarcomAsync("Marcos");

        ContaFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();

        // Act
        await formPage.NomeCliente.FillAsync("Carlos");
        await formPage.Garcom.SelectOptionAsync(new[] { "Marcos" });
        await formPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(formPage.UrlAbrir);
        await Expect(formPage.Erros).ToContainTextAsync("O campo \"Mesa\" é obrigatório.");
    }

    [TestMethod]
    public async Task Deve_RejeitarAbertura_QuandoGarcomNaoSelecionado()
    {
        // Arrange
        await RegistrarEEntrarAsync("conta.garcom.vazio@teste.local", SenhaValida);
        await CadastrarMesaAsync(1, 4);

        ContaFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();

        // Act
        await formPage.NomeCliente.FillAsync("Carlos");
        await formPage.Mesa.SelectOptionAsync(new[] { "Mesa 1 - 4 lugares" });
        await formPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(formPage.UrlAbrir);
        await Expect(formPage.Erros).ToContainTextAsync("O campo \"Garçom\" é obrigatório.");
    }

    [TestMethod]
    public async Task Deve_PermitirDuasContasNaMesmaMesa()
    {
        // Arrange
        await RegistrarEEntrarAsync("conta.mesma.mesa@teste.local", SenhaValida);
        await CadastrarMesaAsync(5, 4);
        await CadastrarGarcomAsync("Marcos");

        ContaListarPage listarPage = new(Page, UrlBase);
        await listarPage.IrParaAsync();

        // Act
        await AbrirContaAsync("Carlos", 5, 4, "Marcos");
        await AbrirContaAsync("Maria", 5, 4, "Marcos");

        // Assert
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        Assert.AreEqual(2, await listarPage.LinhasAbertas.CountAsync());
        await Expect(listarPage.LinhaAbertaPorCliente("Carlos")).ToBeVisibleAsync();
        await Expect(listarPage.LinhaAbertaPorCliente("Maria")).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_ManterMesaOcupada_AteFechamentoDaUltimaConta()
    {
        // Arrange
        await RegistrarEEntrarAsync("conta.mesa.ocupada@teste.local", SenhaValida);
        await CadastrarMesaAsync(1, 4);
        await CadastrarGarcomAsync("Marcos");

        await AbrirContaAsync("Carlos", 1, 4, "Marcos");
        await AbrirContaAsync("Maria", 1, 4, "Marcos");

        ContaListarPage contaListarPage = new(Page, UrlBase);
        MesaListarPage mesaListarPage = new(Page, UrlBase);

        await Expect(contaListarPage.LinhasAbertas).ToHaveCountAsync(2);

        // Act - verificar a mesa ocupada antes e depois da primeira conta
        await mesaListarPage.IrParaAsync();
        await Expect(mesaListarPage.StatusDaMesa(1)).ToHaveTextAsync("Ocupada");

        await contaListarPage.IrParaAsync();
        await contaListarPage.FecharAsync("Carlos");
        await Page.GetByRole(
            AriaRole.Button,
            new() { Name = "Confirmar Fechamento", Exact = true }
        ).ClickAsync();

        await mesaListarPage.IrParaAsync();
        await Expect(mesaListarPage.StatusDaMesa(1)).ToHaveTextAsync("Ocupada");

        // Act - fechar a última conta
        await contaListarPage.IrParaAsync();
        await contaListarPage.FecharAsync("Maria");
        await Page.GetByRole(
            AriaRole.Button,
            new() { Name = "Confirmar Fechamento", Exact = true }
        ).ClickAsync();

        // Assert
        await mesaListarPage.IrParaAsync();
        await Expect(mesaListarPage.StatusDaMesa(1)).ToHaveTextAsync("Livre");
    }

    [TestMethod]
    public async Task Deve_AcessarDetalhes_DeContaAberta()
    {
        // Arrange
        await RegistrarEEntrarAsync("conta.detalhes@teste.local", SenhaValida);
        await CadastrarMesaAsync(2, 4);
        await CadastrarGarcomAsync("Marcos");
        await AbrirContaAsync("Carlos", 2, 4, "Marcos");

        ContaListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.AbrirDetalhesAsync("Carlos");

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@".*/Conta/Detalhes/.+"));
        await Expect(Page.GetByText("Carlos")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Mesa 2")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Marcos")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Aberta")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Fechar Conta", Exact = true })).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_FecharConta_EmFluxoCompleto()
    {
        // Arrange
        await RegistrarEEntrarAsync("conta.fechar@teste.local", SenhaValida);
        await CadastrarMesaAsync(3, 4);
        await CadastrarGarcomAsync("Marcos");
        await AbrirContaAsync("Carlos", 3, 4, "Marcos");

        ContaListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.FecharAsync("Carlos");
        await Expect(Page).ToHaveURLAsync(new Regex(@".*/Conta/Fechar/[0-9a-fA-F-]{36}"));
        await Page.GetByRole(AriaRole.Button, new() { Name = "Confirmar Fechamento", Exact = true }).ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.LinhaAbertaPorCliente("Carlos")).Not.ToBeVisibleAsync();
        await Expect(listarPage.LinhaFechadaPorCliente("Carlos")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Data de Fechamento", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(listarPage.LinhaFechadaPorCliente("Carlos")).ToContainTextAsync(new Regex(@"\d{2}/\d{2}/\d{4} \d{2}:\d{2}"));
    }

    private async Task CadastrarMesaAsync(int numero, int lugares)
    {
        MesaFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(numero, lugares);
        await formPage.ConfirmarAsync();
        await Expect(Page).ToHaveURLAsync($"{UrlBase}/Mesa/Listar");
    }

    private async Task CadastrarGarcomAsync(string nome)
    {
        GarcomFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(nome);
        await formPage.ConfirmarAsync();
        await Expect(Page).ToHaveURLAsync($"{UrlBase}/Garcom/Listar");
    }

    private async Task AbrirContaAsync(string nomeCliente, int numeroMesa, int lugaresMesa, string nomeGarcom)
    {
        ContaFormPage formPage = new(Page, UrlBase);
        ContaListarPage listarPage = new(Page, UrlBase);

        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(nomeCliente, numeroMesa, lugaresMesa, nomeGarcom);
        await formPage.ConfirmarAsync();

        await Expect(Page).ToHaveURLAsync(listarPage.Url);
    }
}
