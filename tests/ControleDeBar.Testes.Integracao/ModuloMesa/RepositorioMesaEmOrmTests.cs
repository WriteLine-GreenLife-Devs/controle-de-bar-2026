using ControleDeBar.Dominio.Compartilhado.Identity;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using ControleDeBar.Infra.Compartilhado.Orm;
using ControleDeBar.Infra.Modulos.ModuloMesa;
using ControleDeBar.Infra.Modulos.ModuloProduto;
using ControleDeBar.Testes.Integracao.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ControleDeBar.Testes.Integracao.ModuloMesa;

[TestClass]
public sealed class RepositorioMesaEmOrmTests : RepositorioBaseEmOrmTests
{
    [TestMethod]
    public void CadastrarESelecionarPorId_CarregaRegistro()
    {
        // Arrange
        Mesa mesa = new(1, 4);

        // Act
        repositorioMesa.Cadastrar(mesa);
        dbContext.ChangeTracker.Clear();

        Mesa? mesaSelecionada = repositorioMesa.SelecionarPorId(mesa.Id);

        // Assert
        Assert.IsNotNull(mesaSelecionada);
        Assert.AreEqual(1, mesaSelecionada.Numero);
        Assert.AreEqual(4, mesaSelecionada.Lugares);
        Assert.AreEqual(StatusMesa.Livre, mesaSelecionada.Status);
    }

    [TestMethod]
    public void Cadastrar_PreencheUserIdDoUsuarioAutenticado()
    {
        // Arrange
        Mesa mesa = new(1, 4);

        // Act
        repositorioMesa.Cadastrar(mesa);
        dbContext.ChangeTracker.Clear();

        Mesa? mesaSelecionada = repositorioMesa.SelecionarPorId(mesa.Id);

        // Assert
        Assert.IsNotNull(mesaSelecionada);
        Assert.AreEqual(userId, mesaSelecionada.UserId);
    }

    [TestMethod]
    public void Editar_AtualizaRegistroEPreservaStatus()
    {
        // Arrange
        Mesa mesa = new(1, 4);
        mesa.Ocupar();
        repositorioMesa.Cadastrar(mesa);

        Mesa mesaAtualizada = new(2, 6);

        // Act
        bool conseguiuEditar = repositorioMesa.Editar(mesa.Id, mesaAtualizada);
        dbContext.ChangeTracker.Clear();

        Mesa? mesaSelecionada = repositorioMesa.SelecionarPorId(mesa.Id);

        // Assert
        Assert.IsTrue(conseguiuEditar);
        Assert.IsNotNull(mesaSelecionada);
        Assert.AreEqual(2, mesaSelecionada.Numero);
        Assert.AreEqual(6, mesaSelecionada.Lugares);
        Assert.AreEqual(StatusMesa.Ocupada, mesaSelecionada.Status);
    }

    [TestMethod]
    public void Excluir_RemoveRegistroExistente()
    {
        // Arrange
        Mesa mesa = new(1, 4);
        repositorioMesa.Cadastrar(mesa);

        // Act
        bool conseguiuExcluir = repositorioMesa.Excluir(mesa.Id);
        dbContext.ChangeTracker.Clear();

        // Assert
        Assert.IsTrue(conseguiuExcluir);
        Assert.IsNull(repositorioMesa.SelecionarPorId(mesa.Id));
    }

    [TestMethod]
    public void SelecionarTodos_CarregaSomenteRegistrosDoUsuarioAutenticado()
    {
        // Arrange
        repositorioMesa.Cadastrar(new Mesa(1, 4));
        repositorioMesa.Cadastrar(new Mesa(2, 6));

        Guid outroUsuarioId = Guid.CreateVersion7();

        using (ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId))
        {
            RepositorioMesaEmOrm outroRepositorio = new(outroContexto);
            outroRepositorio.Cadastrar(new Mesa(3, 8));
        }

        dbContext.ChangeTracker.Clear();

        // Act
        List<Mesa> mesas = repositorioMesa.SelecionarTodos();

        // Assert
        Assert.HasCount(2, mesas);
        Assert.IsTrue(mesas.All(m => m.UserId == userId));
    }

    [TestMethod]
    public void Cadastrar_PermiteMesmoNumeroParaUsuariosDiferentes()
    {
        // Arrange
        repositorioMesa.Cadastrar(new Mesa(1, 4));

        Guid outroUsuarioId = Guid.CreateVersion7();

        // Act
        using ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId);
        RepositorioMesaEmOrm outroRepositorio = new(outroContexto);

        outroRepositorio.Cadastrar(new Mesa(1, 6));

        // Assert
        Assert.HasCount(1, repositorioMesa.SelecionarTodos());
        Assert.HasCount(1, outroRepositorio.SelecionarTodos());
        Assert.AreEqual(4, repositorioMesa.SelecionarTodos()[0].Lugares);
        Assert.AreEqual(6, outroRepositorio.SelecionarTodos()[0].Lugares);
    }

    [TestMethod]
    public void SelecionarPorId_NaoCarregaMesaDeOutroUsuario()
    {
        // Arrange
        Guid outroUsuarioId = Guid.CreateVersion7();
        Mesa mesaOutroUsuario = new(1, 4);

        using (ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId))
        {
            new RepositorioMesaEmOrm(outroContexto).Cadastrar(mesaOutroUsuario);
        }

        dbContext.ChangeTracker.Clear();

        // Act
        Mesa? mesaSelecionada = repositorioMesa.SelecionarPorId(mesaOutroUsuario.Id);

        // Assert
        Assert.IsNull(mesaSelecionada);
    }

    [TestMethod]
    public void SaveChanges_RejeitaModificacaoDeMesaDeOutroUsuario()
    {
        // Arrange
        Mesa mesaOutroUsuario = new(1, 4)
        {
            UserId = Guid.CreateVersion7()
        };

        dbContext.Attach(mesaOutroUsuario);
        mesaOutroUsuario.Lugares = 8;

        // Act
        Action salvarAlteracoes = () => dbContext.SaveChanges();

        // Assert
        Assert.Throws<UnauthorizedAccessException>(salvarAlteracoes);
    }

    [TestMethod]
    public void SaveChanges_RejeitaExclusaoDeMesaDeOutroUsuario()
    {
        // Arrange
        Mesa mesaOutroUsuario = new(1, 4)
        {
            UserId = Guid.CreateVersion7()
        };

        dbContext.Attach(mesaOutroUsuario);
        dbContext.Remove(mesaOutroUsuario);

        // Act
        Action salvarAlteracoes = () => dbContext.SaveChanges();

        // Assert
        Assert.Throws<UnauthorizedAccessException>(salvarAlteracoes);
    }

    [TestMethod]
    public void Deve_PreservarEstadoDoBanco_AposFalhaDeSaveChanges()
    {
        // Arrange
        Mesa mesa = new(4, 4);
        repositorioMesa.Cadastrar(mesa);
        dbContext.ChangeTracker.Clear();

        Guid outroUsuarioId = Guid.CreateVersion7();
        using (ControleDeBarDbContext contextoInvalido = CriarDbContext(outroUsuarioId))
        {
            Mesa mesaAlterada = new(4, 4)
            {
                Id = mesa.Id,
                UserId = userId
            };

            contextoInvalido.Attach(mesaAlterada);
            mesaAlterada.Numero = 99;
            mesaAlterada.Lugares = 99;

            // Act
            Action salvarAlteracoes = () => contextoInvalido.SaveChanges();

            // Assert
            Assert.Throws<UnauthorizedAccessException>(salvarAlteracoes);
        }

        dbContext.ChangeTracker.Clear();
        Mesa? mesaPersistida = repositorioMesa.SelecionarPorId(mesa.Id);

        Assert.IsNotNull(mesaPersistida);
        Assert.AreEqual(4, mesaPersistida.Numero);
        Assert.AreEqual(4, mesaPersistida.Lugares);
    }

    [TestMethod]
    public void Modelo_ConfiguraIndiceUnicoPorUsuarioENumero()
    {
        // Arrange
        IEntityType tipoMesa = dbContext.Model.FindEntityType(typeof(Mesa))!;

        // Act
        IIndex? indice = tipoMesa.GetIndexes().SingleOrDefault(i =>
            i.Properties.Select(p => p.Name)
                .SequenceEqual([nameof(IEntidadeDoUsuario.UserId), nameof(Mesa.Numero)])
        );

        // Assert
        Assert.IsNotNull(indice);
        Assert.IsTrue(indice.IsUnique);
        Assert.AreEqual("UQ_TBMesa_UserId_Numero", indice.GetDatabaseName());
    }
}
