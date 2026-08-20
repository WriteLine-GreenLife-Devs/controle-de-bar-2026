using ControleDeBar.Testes.E2E.Compartilhado;
using ControleDeBar.Testes.E2E.Modulos.ModuloAutenticacao;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloMesa;

[TestClass]
public sealed class MesaE2ETests : E2ETestsBase
{
    private const string SenhaValida = "Senha123!";

    [TestMethod]
    public async Task Deve_ExibirListagemVazia_ParaUsuarioSemMesas()
    {
        // Arrange
        await RegistrarEEntrarAsync("mesa.vazia@teste.local", SenhaValida);
        MesaListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.Titulo).ToBeVisibleAsync();
        await Expect(listarPage.CadastrarMesa).ToBeVisibleAsync();
        await Expect(listarPage.EstadoVazio).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_ExecutarCrudCompleto_DeMesa()
    {
        // Arrange
        await RegistrarEEntrarAsync("mesa.crud@teste.local", SenhaValida);

        MesaFormPage formPage = new(Page, UrlBase);
        MesaListarPage listarPage = new(Page, UrlBase);
        MesaExcluirPage excluirPage = new(Page);

        // Act - cadastrar
        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(1, 4);
        await formPage.ConfirmarAsync();

        // Assert - cadastrar
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.CardPorNumero(1)).ToBeVisibleAsync();
        await Expect(listarPage.StatusDaMesa(1)).ToHaveTextAsync("Livre");

        // Act - editar
        await listarPage.EditarAsync(1);
        await formPage.PreencherAsync(2, 6);
        await formPage.ConfirmarAsync();

        // Assert - editar
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.CardPorNumero(1)).Not.ToBeVisibleAsync();
        await Expect(listarPage.CardPorNumero(2)).ToContainTextAsync("6 lugares");

        // Act - excluir
        await listarPage.ExcluirAsync(2);
        await Expect(excluirPage.MensagemConfirmacao).ToBeVisibleAsync();
        await excluirPage.ConfirmarAsync();

        // Assert - excluir
        await Expect(Page).ToHaveURLAsync(listarPage.Url);
        await Expect(listarPage.CardPorNumero(2)).Not.ToBeVisibleAsync();
        await Expect(listarPage.EstadoVazio).ToBeVisibleAsync();
    }

    [TestMethod]
    [DataRow(0, 4, "O campo \"Número\" deve ser maior que zero.")]
    [DataRow(1, 0, "O campo \"Lugares\" deve ser maior que zero.")]
    public async Task Deve_RejeitarCadastro_DeMesaComDadosInvalidos(
        int numero,
        int lugares,
        string mensagemEsperada
    )
    {
        // Arrange
        await RegistrarEEntrarAsync(
            $"mesa.invalida.{numero}.{lugares}@teste.local",
            SenhaValida
        );

        MesaFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();

        // Act
        await formPage.PreencherAsync(numero, lugares);
        await formPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(formPage.UrlCadastrar);
        await Expect(formPage.Erros).ToContainTextAsync(mensagemEsperada);
    }

    [TestMethod]
    public async Task Deve_RejeitarCadastro_DeMesaComNumeroDuplicado()
    {
        // Arrange
        await RegistrarEEntrarAsync("mesa.duplicada@teste.local", SenhaValida);
        await CadastrarMesaAsync(1, 4);

        MesaFormPage formPage = new(Page, UrlBase);
        await formPage.IrParaCadastroAsync();

        // Act
        await formPage.PreencherAsync(1, 6);
        await formPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(formPage.UrlCadastrar);
        await Expect(formPage.Erros).ToContainTextAsync("Já existe uma mesa com este número.");
    }

    [TestMethod]
    public async Task Deve_IsolarMesas_EntreUsuariosDiferentes()
    {
        // Arrange
        const string primeiroEmail = "mesa.usuario.a@teste.local";

        await RegistrarEEntrarAsync(primeiroEmail, SenhaValida);
        await CadastrarMesaAsync(1, 4);

        EntrarPage entrarPage = new(Page, UrlBase);
        await entrarPage.SairAsync(primeiroEmail);

        RegistrarPage registrarPage = new(Page, UrlBase);
        await registrarPage.IrParaAsync();
        await registrarPage.PreencherAsync("mesa.usuario.b@teste.local", SenhaValida);
        await registrarPage.ConfirmarAsync();

        MesaListarPage listarPage = new(Page, UrlBase);

        // Act
        await listarPage.IrParaAsync();

        // Assert
        await Expect(listarPage.CardPorNumero(1)).Not.ToBeVisibleAsync();
        await Expect(listarPage.EstadoVazio).ToBeVisibleAsync();
    }

    private async Task CadastrarMesaAsync(int numero, int lugares)
    {
        MesaFormPage formPage = new(Page, UrlBase);
        MesaListarPage listarPage = new(Page, UrlBase);

        await formPage.IrParaCadastroAsync();
        await formPage.PreencherAsync(numero, lugares);
        await formPage.ConfirmarAsync();

        await Expect(Page).ToHaveURLAsync(listarPage.Url);
    }
}
