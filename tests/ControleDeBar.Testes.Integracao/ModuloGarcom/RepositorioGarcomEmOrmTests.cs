using ControleDeBar.Dominio.Compartilhado.Identity;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Infra.Compartilhado.Orm;
using ControleDeBar.Infra.Modulos.ModuloGarcom;
using ControleDeBar.Testes.Integracao.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ControleDeBar.Testes.Integracao.ModuloGarcom;

[TestClass]
public sealed class RepositorioGarcomEmOrmTests : RepositorioBaseEmOrmTests
{
    [TestMethod]
    public void CadastrarESelecionarPorId_CarregaRegistro()
    {
        // Arrange
        Garcom garcom = new("  Marcos  ");

        // Act
        repositorioGarcom.Cadastrar(garcom);
        dbContext.ChangeTracker.Clear();

        Garcom? garcomSelecionado = repositorioGarcom.SelecionarPorId(garcom.Id);

        // Assert
        Assert.IsNotNull(garcomSelecionado);
        Assert.AreEqual("Marcos", garcomSelecionado.Nome);
    }

    [TestMethod]
    public void Cadastrar_PreencheUserIdDoUsuarioAutenticado()
    {
        // Arrange
        Garcom garcom = new("Marcos");

        // Act
        repositorioGarcom.Cadastrar(garcom);
        dbContext.ChangeTracker.Clear();

        Garcom? garcomSelecionado = repositorioGarcom.SelecionarPorId(garcom.Id);

        // Assert
        Assert.IsNotNull(garcomSelecionado);
        Assert.AreEqual(userId, garcomSelecionado.UserId);
    }

    [TestMethod]
    public void Editar_AtualizaRegistroEPreservaUserId()
    {
        // Arrange
        Garcom garcom = new("Marcos");
        repositorioGarcom.Cadastrar(garcom);

        Garcom garcomAtualizado = new("Paulo")
        {
            UserId = Guid.CreateVersion7()
        };

        // Act
        bool conseguiuEditar = repositorioGarcom.Editar(garcom.Id, garcomAtualizado);
        dbContext.ChangeTracker.Clear();

        Garcom? garcomSelecionado = repositorioGarcom.SelecionarPorId(garcom.Id);

        // Assert
        Assert.IsTrue(conseguiuEditar);
        Assert.IsNotNull(garcomSelecionado);
        Assert.AreEqual("Paulo", garcomSelecionado.Nome);
        Assert.AreEqual(userId, garcomSelecionado.UserId);
    }

    [TestMethod]
    public void Excluir_RemoveRegistroExistente()
    {
        // Arrange
        Garcom garcom = new("Marcos");
        repositorioGarcom.Cadastrar(garcom);

        // Act
        bool conseguiuExcluir = repositorioGarcom.Excluir(garcom.Id);
        dbContext.ChangeTracker.Clear();

        // Assert
        Assert.IsTrue(conseguiuExcluir);
        Assert.IsNull(repositorioGarcom.SelecionarPorId(garcom.Id));
    }

    [TestMethod]
    public void SelecionarTodos_CarregaSomenteRegistrosDoUsuarioAutenticado()
    {
        // Arrange
        repositorioGarcom.Cadastrar(new Garcom("Marcos"));
        repositorioGarcom.Cadastrar(new Garcom("Paulo"));

        Guid outroUsuarioId = Guid.CreateVersion7();

        using (ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId))
        {
            RepositorioGarcomEmOrm outroRepositorio = new(outroContexto);
            outroRepositorio.Cadastrar(new Garcom("Rafael"));
        }

        dbContext.ChangeTracker.Clear();

        // Act
        List<Garcom> garcons = repositorioGarcom.SelecionarTodos();

        // Assert
        Assert.HasCount(2, garcons);
        Assert.IsTrue(garcons.All(g => g.UserId == userId));
    }

    [TestMethod]
    public void Cadastrar_PermiteMesmoNomeParaUsuariosDiferentes()
    {
        // Arrange
        repositorioGarcom.Cadastrar(new Garcom("Marcos"));

        Guid outroUsuarioId = Guid.CreateVersion7();

        // Act
        using ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId);
        RepositorioGarcomEmOrm outroRepositorio = new(outroContexto);

        outroRepositorio.Cadastrar(new Garcom("Marcos"));

        // Assert
        Assert.HasCount(1, repositorioGarcom.SelecionarTodos());
        Assert.HasCount(1, outroRepositorio.SelecionarTodos());
        Assert.AreEqual(userId, repositorioGarcom.SelecionarTodos()[0].UserId);
        Assert.AreEqual(outroUsuarioId, outroRepositorio.SelecionarTodos()[0].UserId);
    }

    [TestMethod]
    public void SelecionarPorId_NaoCarregaGarcomDeOutroUsuario()
    {
        // Arrange
        Guid outroUsuarioId = Guid.CreateVersion7();
        Garcom garcomOutroUsuario = new("Marcos");

        using (ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId))
        {
            new RepositorioGarcomEmOrm(outroContexto).Cadastrar(garcomOutroUsuario);
        }

        dbContext.ChangeTracker.Clear();

        // Act
        Garcom? garcomSelecionado = repositorioGarcom.SelecionarPorId(garcomOutroUsuario.Id);

        // Assert
        Assert.IsNull(garcomSelecionado);
    }

    [TestMethod]
    public void SaveChanges_RejeitaModificacaoDeGarcomDeOutroUsuario()
    {
        // Arrange
        Garcom garcomOutroUsuario = new("Marcos")
        {
            UserId = Guid.CreateVersion7()
        };

        dbContext.Attach(garcomOutroUsuario);
        garcomOutroUsuario.Nome = "Paulo";

        // Act
        Action salvarAlteracoes = () => dbContext.SaveChanges();

        // Assert
        Assert.Throws<UnauthorizedAccessException>(salvarAlteracoes);
    }

    [TestMethod]
    public void SaveChanges_RejeitaExclusaoDeGarcomDeOutroUsuario()
    {
        // Arrange
        Garcom garcomOutroUsuario = new("Marcos")
        {
            UserId = Guid.CreateVersion7()
        };

        dbContext.Attach(garcomOutroUsuario);
        dbContext.Remove(garcomOutroUsuario);

        // Act
        Action salvarAlteracoes = () => dbContext.SaveChanges();

        // Assert
        Assert.Throws<UnauthorizedAccessException>(salvarAlteracoes);
    }

    [TestMethod]
    public void Modelo_ConfiguraIndiceUnicoPorUsuarioENome()
    {
        // Arrange
        IEntityType tipoGarcom = dbContext.Model.FindEntityType(typeof(Garcom))!;

        // Act
        IIndex? indice = tipoGarcom.GetIndexes().SingleOrDefault(i =>
            i.Properties.Select(p => p.Name)
                .SequenceEqual([nameof(IEntidadeDoUsuario.UserId), nameof(Garcom.Nome)])
        );

        // Assert
        Assert.IsNotNull(indice);
        Assert.IsTrue(indice.IsUnique);
        Assert.AreEqual("UQ_TBGarcom_UserId_Nome", indice.GetDatabaseName());
    }
}
