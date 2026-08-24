using ControleDeBar.Testes.E2E.Compartilhado;
using ControleDeBar.Testes.E2E.Modulos.ModuloAutenticacao;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloProduto;
using Microsoft.Playwright;

[TestClass]
public sealed class ProdutoE2ETests : E2ETestsBase
{
    private const string SenhaValida = "Senha123!";

    [TestMethod]
    public async Task Deve_ExibirListagemVazia_ParaUsuarioSemProdutos()
    {
        // Arrange
        await RegistrarEEntrarAsync("produto.vazia@teste.local", SenhaValida);
        ProdutoListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.Titulo).ToBeVisibleAsync();
        await Expect(listarPage.CadastrarProduto).ToBeVisibleAsync();
        await Expect(listarPage.EstadoVazio).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_ExecutarCrudCompleto_DeProduto()
    {
        // Arrange
        await RegistrarEEntrarAsync("produto.crud@teste.local", SenhaValida);

        ProdutoFormPage formPage = new(Page, UrlBase);
        ProdutoListarPage listarPage = new(Page, UrlBase);
        ProdutoExcluirPage excluirPage = new(Page);

        // Act - cadastrar
        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync("Refrigerante", "5.50");
        await formPage.ConfirmarAsync();

        // Assert - cadastrar
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        ILocator linhas = listarPage.LinhasProdutos();
        await Expect(linhas).ToContainTextAsync("Refrigerante");
        await Expect(linhas).ToContainTextAsync("R$ 5,50");

        // Act - editar
        await listarPage.EditarAsync("Refrigerante");
        await formPage.PreencherAsync("Suco", "7.00");
        await formPage.ConfirmarAsync();

        // Assert - editar
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.LinhasProdutos()).ToContainTextAsync("Suco");
        await Expect(listarPage.LinhasProdutos()).ToContainTextAsync("R$ 7,00");

        // Act - excluir
        await listarPage.ExcluirAsync("Suco");
        await Expect(excluirPage.MensagemConfirmacao).ToBeVisibleAsync();
        await excluirPage.ConfirmarAsync();

        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.EstadoVazio).ToBeVisibleAsync();
    }

    [TestMethod]
    [DataRow("", "10.00", "O campo \"Nome\" é obrigatório.")]
    [DataRow("Cerveja", "0", "")]
    public async Task Deve_RejeitarCadastro_DeProdutoComDadosInvalidos(
        string nome,
        string preco,
        string mensagemEsperada
    )
    {
        // Arrange
        await RegistrarEEntrarAsync(
            $"produto.invalida.{nome}.{preco}@teste.local",
            SenhaValida
        );

        ProdutoFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();

        // Act
        await formPage.PreencherAsync(nome, preco);
        await formPage.ConfirmarAsync();

        await Expect(Page).ToHaveURLAsync(formPage.UrlCadastrar);
        if (!string.IsNullOrEmpty(mensagemEsperada))
            await Expect(formPage.Erros).ToContainTextAsync(mensagemEsperada);
        else
        {
            Assert.IsFalse(
                await formPage.Preco.EvaluateAsync<bool>("element => element.validity.valid")
            );
            Assert.IsTrue(
                await formPage.Preco.EvaluateAsync<bool>("element => element.validity.rangeUnderflow")
            );
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(
                    await formPage.Preco.EvaluateAsync<string>("element => element.validationMessage")
                )
            );
        }
    }

    [TestMethod]
    public async Task Deve_RejeitarCadastro_DeProdutoComNomeDuplicado()
    {
        // Arrange
        await RegistrarEEntrarAsync("produto.duplicada@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Vinho", "45.50");

        ProdutoFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();

        // Act
        await formPage.PreencherAsync("Vinho", "55.00");
        await formPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(formPage.UrlCadastrar);
        await Expect(formPage.Erros).ToContainTextAsync("Já existe um produto com este nome.");
    }

    [TestMethod]
    public async Task Deve_PermitirManteroNomeDuranteEdicao()
    {
        // Arrange
        await RegistrarEEntrarAsync("produto.manter.nome@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Café", "12.50");

        ProdutoFormPage formPage = new(Page, UrlBase);
        ProdutoListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();
        await listarPage.EditarAsync("Café");
        await formPage.PreencherAsync("Café", "14.00");
        await formPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.LinhasProdutos()).ToContainTextAsync("Café");
        await Expect(listarPage.LinhasProdutos()).ToContainTextAsync("R$ 14,00");
    }

    [TestMethod]
    public async Task Deve_RejeitarEdicao_ComNomeDuplicado()
    {
        // Arrange
        await RegistrarEEntrarAsync("produto.edit.duplicada@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Chopp", "25.00");
        await CadastrarProdutoAsync("Vinho Tinto", "50.00");

        ProdutoFormPage formPage = new(Page, UrlBase);
        ProdutoListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();
        await listarPage.EditarAsync("Chopp");
        await formPage.PreencherAsync("Vinho Tinto", "30.00");
        await formPage.ConfirmarAsync();

        // Assert
        StringAssert.Contains(Page.Url, "/Produto/Editar/");
        await Expect(formPage.Erros).ToContainTextAsync("Já existe um produto com este nome.");
    }

    [TestMethod]
    public async Task Deve_BuscarProduto_PorNome()
    {
        // Arrange
        await RegistrarEEntrarAsync("produto.busca@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Água", "3.50");
        await CadastrarProdutoAsync("Cerveja Premium", "12.00");
        await CadastrarProdutoAsync("Suco Natural", "8.00");

        ProdutoListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();
        await listarPage.BuscarAsync("Cerveja");

        // Assert
        int linhas = await listarPage.LinhasProdutos().CountAsync();
        await Expect(listarPage.LinhasProdutos()).ToContainTextAsync("Cerveja Premium");
        Assert.AreEqual(1, linhas, "Deve retornar apenas um produto na busca");
    }

    [TestMethod]
    public async Task Deve_BuscarProduto_CaseInsensitive()
    {
        // Arrange
        await RegistrarEEntrarAsync("produto.busca.case@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Refrigerante", "5.50");

        ProdutoListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();
        await listarPage.BuscarAsync("refrigerante");

        // Assert
        await Expect(listarPage.LinhasProdutos()).ToContainTextAsync("Refrigerante");
    }

    [TestMethod]
    public async Task Deve_RetornarTodos_QuandoBuscaForVazia()
    {
        // Arrange
        await RegistrarEEntrarAsync("produto.busca.vazia@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Chopp", "20.00");
        await CadastrarProdutoAsync("Refrigerante", "5.50");

        ProdutoListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();
        await listarPage.BuscarAsync("Chopp");
        await listarPage.LimparBuscaAsync();

        // Assert
        int linhas = await listarPage.LinhasProdutos().CountAsync();
        Assert.AreEqual(2, linhas, "Deve retornar todos os produtos quando a busca for vazia");
    }

    [TestMethod]
    public async Task Deve_IsolarProdutos_EntreUsuariosDiferentes()
    {
        // Arrange
        const string primeiroEmail = "produto.usuario.a@teste.local";

        await RegistrarEEntrarAsync(primeiroEmail, SenhaValida);
        await CadastrarProdutoAsync("Whisky", "80.00");

        EntrarPage entrarPage = new(Page, UrlBase);
        await entrarPage.SairAsync(primeiroEmail);

        RegistrarPage registrarPage = new(Page, UrlBase);
        await registrarPage.IrParaAsync();
        await registrarPage.PreencherAsync("produto.usuario.b@teste.local", SenhaValida);
        await registrarPage.ConfirmarAsync();

        ProdutoListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();

        // Assert
        Assert.AreEqual(0, await listarPage.LinhasProdutos().CountAsync());
        await Expect(listarPage.EstadoVazio).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_AcessarProdutosViaMenu()
    {
        // Arrange
        await RegistrarEEntrarAsync("produto.menu@teste.local", SenhaValida);
        await CadastrarProdutoAsync("Água com Gás", "4.00");

        ProdutoListarPage listarPage = new(Page, UrlBase);

        // Act
        await Page.GetByRole(
            AriaRole.Link,
            new() { Name = "Produtos", Exact = true }
        ).ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.Titulo).ToBeVisibleAsync();
        await Expect(listarPage.LinhasProdutos()).ToContainTextAsync("Água com Gás");
    }

    private async Task CadastrarProdutoAsync(string nome, string preco)
    {
        ProdutoFormPage formPage = new(Page, UrlBase);
        ProdutoListarPage listarPage = new(Page, UrlBase);

        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(nome, preco);
        await formPage.ConfirmarAsync();

        await Expect(Page).ToHaveURLAsync(listarPage.Url);
    }


}
