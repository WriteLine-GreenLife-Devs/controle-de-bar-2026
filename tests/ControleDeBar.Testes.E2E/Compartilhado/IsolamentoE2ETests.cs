using System.Globalization;
using System.Text.RegularExpressions;
using ControleDeBar.Testes.E2E.Modulos.ModuloAutenticacao;
using ControleDeBar.Testes.E2E.Modulos.ModuloConta;
using ControleDeBar.Testes.E2E.Modulos.ModuloFaturamento;
using ControleDeBar.Testes.E2E.Modulos.ModuloGarcom;
using ControleDeBar.Testes.E2E.Modulos.ModuloMesa;
using ControleDeBar.Testes.E2E.Modulos.ModuloPedido;
using ControleDeBar.Testes.E2E.Modulos.ModuloProduto;
using Microsoft.Playwright;

namespace ControleDeBar.Testes.E2E.Compartilhado;

[TestClass]
public sealed class IsolamentoE2ETests : E2ETestsBase
{
    private const string SenhaValida = "Senha123!";

    [TestMethod]
    public async Task Deve_IsolarVisualmente_DadosEntreEstabelecimentos()
    {
        // Arrange - usuário A cria dados em todos os módulos relacionados
        const string emailUsuarioA = "isolamento.usuario.a@teste.local";
        const string emailUsuarioB = "isolamento.usuario.b@teste.local";

        await RegistrarEEntrarAsync(emailUsuarioA, SenhaValida);
        await CadastrarMesaAsync(1, 4);
        await CadastrarGarcomAsync("Garçom A");
        await CadastrarProdutoAsync("Produto A", 8.50m);
        Guid contaId = await AbrirContaAsync("Cliente A", 1, 4, "Garçom A");
        await AdicionarPedidoAsync(contaId, "Produto A", 8.50m, 2);
        await FecharContaAsync(contaId);

        FaturamentoPage faturamentoPage = new(Page, UrlBase);
        await faturamentoPage.IrParaAsync();
        await faturamentoPage.ConsultarDataAsync(DateTime.Today);
        await Expect(faturamentoPage.LinhasContas.Filter(new() { HasText = "Cliente A" })).ToBeVisibleAsync();
        await Expect(faturamentoPage.Total).ToContainTextAsync("R$ 17,00");

        EntrarPage entrarPage = new(Page, UrlBase);
        await entrarPage.SairAsync(emailUsuarioA);

        // Act - usuário B acessa os mesmos módulos
        await RegistrarEEntrarAsync(emailUsuarioB, SenhaValida);

        MesaListarPage mesaListarPage = new(Page, UrlBase);
        await mesaListarPage.IrParaAsync();
        await Expect(mesaListarPage.CardPorNumero(1)).Not.ToBeVisibleAsync();
        await Expect(mesaListarPage.EstadoVazio).ToBeVisibleAsync();

        GarcomListarPage garcomListarPage = new(Page, UrlBase);
        await garcomListarPage.IrParaAsync();
        await Expect(garcomListarPage.LinhaPorNome("Garçom A")).Not.ToBeVisibleAsync();
        await Expect(garcomListarPage.EstadoVazio).ToBeVisibleAsync();

        ProdutoListarPage produtoListarPage = new(Page, UrlBase);
        await produtoListarPage.IrParaAsync();
        await Expect(produtoListarPage.LinhasProdutos().Filter(new() { HasText = "Produto A" })).Not.ToBeVisibleAsync();
        await Expect(produtoListarPage.EstadoVazio).ToBeVisibleAsync();

        ContaListarPage contaListarPage = new(Page, UrlBase);
        await contaListarPage.IrParaAsync();
        await Expect(contaListarPage.LinhaAbertaPorCliente("Cliente A")).Not.ToBeVisibleAsync();
        await Expect(contaListarPage.LinhaFechadaPorCliente("Cliente A")).Not.ToBeVisibleAsync();
        await Expect(contaListarPage.EstadoVazioAbertas).ToBeVisibleAsync();
        await Expect(contaListarPage.EstadoVazioFechadas).ToBeVisibleAsync();

        await faturamentoPage.IrParaAsync();
        await faturamentoPage.ConsultarDataAsync(DateTime.Today);

        // Assert
        await Expect(faturamentoPage.LinhasContas.Filter(new() { HasText = "Cliente A" })).ToHaveCountAsync(0);
        await Expect(faturamentoPage.Total).ToContainTextAsync("R$ 0,00");
        await Expect(faturamentoPage.EstadoVazio).ToBeVisibleAsync();

        // A Conta de A não pode expor seus pedidos para B
        await Page.GotoAsync(UrlDetalhes(contaId));
        await Expect(Page).ToHaveURLAsync(new Regex(@".*/Conta/Listar$"));
    }

    private async Task CadastrarMesaAsync(int numero, int lugares)
    {
        MesaFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(numero, lugares);
        await formPage.ConfirmarAsync();
    }

    private async Task CadastrarGarcomAsync(string nome)
    {
        GarcomFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(nome);
        await formPage.ConfirmarAsync();
    }

    private async Task CadastrarProdutoAsync(string nome, decimal preco)
    {
        ProdutoFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(nome, preco.ToString("F2", CultureInfo.InvariantCulture));
        await formPage.ConfirmarAsync();
    }

    private async Task<Guid> AbrirContaAsync(string nomeCliente, int numeroMesa, int lugaresMesa, string nomeGarcom)
    {
        ContaFormPage formPage = new(Page, UrlBase);
        ContaListarPage listarPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(nomeCliente, numeroMesa, lugaresMesa, nomeGarcom);
        await formPage.ConfirmarAsync();
        await listarPage.AbrirDetalhesAsync(nomeCliente);
        return Guid.Parse(new Uri(Page.Url).Segments[^1]);
    }

    private async Task AdicionarPedidoAsync(Guid contaId, string nomeProduto, decimal preco, int quantidade)
    {
        PedidoFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaAdicionarAsync(contaId);
        await formPage.SelecionarProdutoAsync(nomeProduto, preco);
        await formPage.Quantidade.FillAsync(quantidade.ToString());
        await formPage.ConfirmarAsync();
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
    }

    private string UrlDetalhes(Guid contaId) => $"{UrlBase}/Conta/Detalhes/{contaId}";
}
