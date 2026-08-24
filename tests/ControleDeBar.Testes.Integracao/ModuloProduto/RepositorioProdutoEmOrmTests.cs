using ControleDeBar.Dominio.Compartilhado.Identity;
using ControleDeBar.Dominio.Modulos.ModuloProduto;
using ControleDeBar.Infra.Compartilhado.Orm;
using ControleDeBar.Infra.Modulos.ModuloProduto;
using ControleDeBar.Testes.Integracao.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ControleDeBar.Testes.Integracao.ModuloProduto;

[TestClass]
public sealed class RepositorioProdutoEmOrmTests : RepositorioBaseEmOrmTests
{
    private RepositorioProdutoEmOrm repositorioProduto = null!;

    [TestInitialize]
    public override void InicializarContexto()
    {
        base.InicializarContexto();
        repositorioProduto = new RepositorioProdutoEmOrm(dbContext);
    }

    [TestMethod]
    public void CadastrarESelecionarPorId_CarregaRegistro()
    {
        // Arrange
        Produto produto = new("Cerveja Premium", 8.50m);

        // Act
        repositorioProduto.Cadastrar(produto);
        dbContext.ChangeTracker.Clear();

        Produto? produtoSelecionado = repositorioProduto.SelecionarPorId(produto.Id);

        // Assert
        Assert.IsNotNull(produtoSelecionado);
        Assert.AreEqual("Cerveja Premium", produtoSelecionado.Nome);
        Assert.AreEqual(8.50m, produtoSelecionado.Preco);
    }

    [TestMethod]
    public void Cadastrar_PreencheUserIdDoUsuarioAutenticado()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);

        // Act
        repositorioProduto.Cadastrar(produto);
        dbContext.ChangeTracker.Clear();

        Produto? produtoSelecionado = repositorioProduto.SelecionarPorId(produto.Id);

        // Assert
        Assert.IsNotNull(produtoSelecionado);
        Assert.AreEqual(userId, produtoSelecionado.UserId);
    }

    [TestMethod]
    public void Editar_AtualizaRegistro()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);
        repositorioProduto.Cadastrar(produto);

        Produto produtoAtualizado = new("Refrigerante", 5.00m);

        // Act
        bool conseguiuEditar = repositorioProduto.Editar(produto.Id, produtoAtualizado);
        dbContext.ChangeTracker.Clear();

        Produto? produtoSelecionado = repositorioProduto.SelecionarPorId(produto.Id);

        // Assert
        Assert.IsTrue(conseguiuEditar);
        Assert.IsNotNull(produtoSelecionado);
        Assert.AreEqual("Refrigerante", produtoSelecionado.Nome);
        Assert.AreEqual(5.00m, produtoSelecionado.Preco);
    }

    [TestMethod]
    public void Excluir_RemoveRegistroExistente()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);
        repositorioProduto.Cadastrar(produto);

        // Act
        bool conseguiuExcluir = repositorioProduto.Excluir(produto.Id);
        dbContext.ChangeTracker.Clear();

        // Assert
        Assert.IsTrue(conseguiuExcluir);
        Assert.IsNull(repositorioProduto.SelecionarPorId(produto.Id));
    }

    [TestMethod]
    public void SelecionarTodos_CarregaSomenteRegistrosDoUsuarioAutenticado()
    {
        // Arrange
        repositorioProduto.Cadastrar(new Produto("Cerveja", 8.50m));
        repositorioProduto.Cadastrar(new Produto("Refrigerante", 5.00m));

        Guid outroUsuarioId = Guid.CreateVersion7();

        using (ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId))
        {
            RepositorioProdutoEmOrm outroRepositorio = new(outroContexto);

            // Act
            outroRepositorio.Cadastrar(new Produto("Suco", 6.00m));

            List<Produto> produtosUsuarioAtual = repositorioProduto.SelecionarTodos();
            List<Produto> produtosOutroUsuario = outroRepositorio.SelecionarTodos();

            // Assert
            Assert.AreEqual(2, produtosUsuarioAtual.Count);
            Assert.AreEqual(1, produtosOutroUsuario.Count);
            Assert.IsFalse(produtosUsuarioAtual.Any(p => p.Nome == "Suco"));
        }
    }

    [TestMethod]
    public void SelecionarPorId_RetornaNuloParaProdutoDeOutroUsuario()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);
        repositorioProduto.Cadastrar(produto);

        Guid outroUsuarioId = Guid.CreateVersion7();

        using (ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId))
        {
            RepositorioProdutoEmOrm outroRepositorio = new(outroContexto);

            // Act
            Produto? produtoSelecionado = outroRepositorio.SelecionarPorId(produto.Id);

            // Assert
            Assert.IsNull(produtoSelecionado);
        }
    }

    [TestMethod]
    public void Excluir_RetornaFalsoParaProdutoInexistente()
    {
        // Arrange
        Guid idInexistente = Guid.CreateVersion7();

        // Act
        bool resultado = repositorioProduto.Excluir(idInexistente);

        // Assert
        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void Editar_RetornaFalsoParaProdutoInexistente()
    {
        // Arrange
        Guid idInexistente = Guid.CreateVersion7();
        Produto produtoAtualizado = new("Cerveja", 8.50m);

        // Act
        bool resultado = repositorioProduto.Editar(idInexistente, produtoAtualizado);

        // Assert
        Assert.IsFalse(resultado);
    }
}
