using ControleDeBar.Testes.E2E.Compartilhado;

namespace ControleDeBar.Testes.E2E.Modulos.ModuloAutenticacao;

[TestClass]
public sealed class AutenticacaoE2ETests : E2ETestsBase
{
    private const string SenhaValida = "Senha123!";

    [TestMethod]
    public async Task Deve_Exibir_TelaDeLogin_ParaUsuarioAnonimo()
    {
        // Arrange
        EntrarPage entrarPage = new(Page, UrlBase);

        // Act
        await Page.GotoAsync($"{UrlBase}/");

        // Assert
        await Expect(entrarPage.Titulo).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_RegistrarEAutenticar_Usuario()
    {
        // Arrange
        const string email = "novo.usuario@teste.local";

        RegistrarPage registrarPage = new(Page, UrlBase);

        await registrarPage.IrParaAsync();

        // Act
        await registrarPage.PreencherAsync(email, SenhaValida);
        await registrarPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync($"{UrlBase}/");
        await Expect(new EntrarPage(Page, UrlBase).UsuarioAutenticado(email)).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_EntrarEAutenticar_Usuario_Valido()
    {
        // Arrange
        const string email = "login.valido@teste.local";

        await RegistrarUsuarioAsync(email, SenhaValida);

        EntrarPage entrarPage = new(Page, UrlBase);

        // Act
        await entrarPage.IrParaAsync();
        await entrarPage.PreencherAsync(email, SenhaValida);
        await entrarPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync($"{UrlBase}/");
        await Expect(entrarPage.UsuarioAutenticado(email)).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_PermanecerNaTelaDeLogin_QuandoSenhaForInvalida()
    {
        // Arrange
        const string email = "login.invalido@teste.local";

        await RegistrarUsuarioAsync(email, SenhaValida);

        EntrarPage entrarPage = new(Page, UrlBase);

        await entrarPage.IrParaAsync();

        // Act
        await entrarPage.PreencherAsync(email, "SenhaInvalida123!");
        await entrarPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(entrarPage.Url);
        await Expect(entrarPage.Erros).ToContainTextAsync("E-mail ou senha inválidos.");
    }

    [TestMethod]
    public async Task Deve_EncerrarSessao_DoUsuarioAutenticado()
    {
        // Arrange
        const string email = "logout@teste.local";

        await RegistrarUsuarioAsync(email, SenhaValida);

        EntrarPage entrarPage = new(Page, UrlBase);

        await entrarPage.IrParaAsync();
        await entrarPage.PreencherAsync(email, SenhaValida);
        await entrarPage.ConfirmarAsync();

        // Act
        await entrarPage.SairAsync(email);

        // Assert
        await Expect(Page).ToHaveURLAsync(entrarPage.Url);
        await Expect(entrarPage.Titulo).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_Rejeitar_RegistroComEmailJaUtilizado()
    {
        // Arrange
        const string email = "email.repetido@teste.local";

        await RegistrarUsuarioAsync(email, SenhaValida);

        RegistrarPage registrarPage = new(Page, UrlBase);

        await registrarPage.IrParaAsync();

        // Act
        await registrarPage.PreencherAsync(email, SenhaValida);
        await registrarPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(registrarPage.Url);
        await Expect(registrarPage.Erros).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Deve_Rejeitar_RegistroComConfirmacaoDeSenhaDiferente()
    {
        // Arrange
        RegistrarPage registrarPage = new(Page, UrlBase);

        await registrarPage.IrParaAsync();

        // Act
        await registrarPage.PreencherAsync(
            "senhas.diferentes@teste.local",
            SenhaValida,
            "OutraSenha123!"
        );
        await registrarPage.ConfirmarAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(registrarPage.Url);
        await Expect(registrarPage.Erros).ToContainTextAsync("As senhas não conferem.");
    }
}
