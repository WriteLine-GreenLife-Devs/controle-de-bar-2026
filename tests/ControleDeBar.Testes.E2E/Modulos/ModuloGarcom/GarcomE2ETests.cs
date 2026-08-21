using ControleDeBar.Testes.E2E.Compartilhado;
using ControleDeBar.Testes.E2E.Modulos.ModuloAutenticacao;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloGarcom;

[TestClass]
public sealed class GarcomE2ETests : E2ETestsBase
{
    private const string SenhaValida = "Senha123!";

    [TestMethod]
    public async Task Deve_ExibirListagemVazia_ParaUsuarioSemGarcons()
    {
        // Arrange
        await RegistrarEEntrarAsync("garcom.vazio@teste.local", SenhaValida);
        GarcomListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.Titulo).ToBeVisibleAsync();
        await Expect(listarPage.CadastrarGarcom).ToBeVisibleAsync();
        await Expect(listarPage.EstadoVazio).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_ExecutarCrudCompleto_DeGarcom()
    {
        // Arrange
        await RegistrarEEntrarAsync("garcom.crud@teste.local", SenhaValida);

        GarcomFormPage formPage = new(Page, UrlBase);
        GarcomListarPage listarPage = new(Page, UrlBase);
        GarcomExcluirPage excluirPage = new(Page);

        // Act - cadastrar
        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync("  Marcos  ");
        await formPage.ConfirmarAsync();

        // Assert - cadastrar
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.LinhaPorNome("Marcos")).ToBeVisibleAsync();

        // Act - editar
        await listarPage.EditarAsync("Marcos");
        await formPage.PreencherAsync("Paula");
        await formPage.ConfirmarAsync();

        // Assert - editar
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.LinhaPorNome("Marcos")).Not.ToBeVisibleAsync();
        await Expect(listarPage.LinhaPorNome("Paula")).ToBeVisibleAsync();

        // Act - excluir
        await listarPage.ExcluirAsync("Paula");
        await Expect(excluirPage.MensagemConfirmacao).ToBeVisibleAsync();
        await excluirPage.ConfirmarAsync();

        // Assert - excluir
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.LinhaPorNome("Paula")).Not.ToBeVisibleAsync();
        await Expect(listarPage.EstadoVazio).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_RejeitarCadastro_DeGarcomSemNome()
    {
        // Arrange
        await RegistrarEEntrarAsync("garcom.invalido@teste.local", SenhaValida);

        GarcomFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();

        // Act
        await formPage.PreencherAsync(string.Empty);
        await formPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(formPage.UrlCadastrar);
        await Expect(formPage.Erros).ToContainTextAsync("O campo \"Nome\" é obrigatório.");
    }

    [TestMethod]
    public async Task Deve_RejeitarCadastro_DeGarcomComNomeDuplicado()
    {
        // Arrange
        await RegistrarEEntrarAsync("garcom.duplicado@teste.local", SenhaValida);
        await CadastrarGarcomAsync("Marcos");

        GarcomFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();

        // Act
        await formPage.PreencherAsync("marcos");
        await formPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(formPage.UrlCadastrar);
        await Expect(formPage.Erros).ToContainTextAsync("Já existe um garçom com este nome.");
    }

    [TestMethod]
    public async Task Deve_IsolarGarcons_EntreUsuariosDiferentes()
    {
        // Arrange
        const string primeiroEmail = "garcom.usuario.a@teste.local";

        await RegistrarEEntrarAsync(primeiroEmail, SenhaValida);
        await CadastrarGarcomAsync("Marcos");

        EntrarPage entrarPage = new(Page, UrlBase);
        await entrarPage.SairAsync(primeiroEmail);

        RegistrarPage registrarPage = new(Page, UrlBase);
        await registrarPage.IrParaAsync();
        await registrarPage.PreencherAsync("garcom.usuario.b@teste.local", SenhaValida);
        await registrarPage.ConfirmarAsync();

        GarcomListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();

        // Assert
        await Expect(listarPage.LinhaPorNome("Marcos")).Not.ToBeVisibleAsync();
        await Expect(listarPage.EstadoVazio).ToBeVisibleAsync();
    }

    private async Task CadastrarGarcomAsync(string nome)
    {
        GarcomFormPage formPage = new(Page, UrlBase);
        GarcomListarPage listarPage = new(Page, UrlBase);

        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(nome);
        await formPage.ConfirmarAsync();

        await Expect(Page).ToHaveURLAsync(listarPage.Url);
    }
}
