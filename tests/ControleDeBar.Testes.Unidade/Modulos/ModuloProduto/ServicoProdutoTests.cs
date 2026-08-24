using ControleDeBar.Aplicacao.Modulos.ModuloProduto;
using ControleDeBar.Dominio.Modulos.ModuloProduto;
using FluentResults;
using Moq;

namespace ControleDeBar.Testes.Unidade.Modulos.ModuloProduto;

[TestClass]
public sealed class ServicoProdutoTests
{
    private Mock<IRepositorioProduto> repositorioProdutoMock = null!;
    private ServicoProduto servicoProduto = null!;

    [TestInitialize]
    public void Inicializar()
    {
        repositorioProdutoMock = new Mock<IRepositorioProduto>();
        servicoProduto = new ServicoProduto(repositorioProdutoMock.Object);
    }

    [TestMethod]
    public void Deve_CadastrarProduto_ComDadosValidos()
    {
        // Arrange
        CadastrarProdutoDto dto = new("Cerveja Premium", 8.50m);

        repositorioProdutoMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        Result resultado = servicoProduto.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioProdutoMock.Verify(r => r.Cadastrar(It.Is<Produto>(p =>
            p.Nome == "Cerveja Premium" &&
            p.Preco == 8.50m
        )), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarCadastro_ComNomeDuplicado()
    {
        // Arrange
        CadastrarProdutoDto dto = new("Cerveja Premium", 8.50m);

        repositorioProdutoMock.Setup(r => r.SelecionarTodos())
            .Returns([new Produto("Cerveja Premium", 8.50m)]);

        // Act
        Result resultado = servicoProduto.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Já existe um produto com este nome.", resultado.Errors[0].Message);
        repositorioProdutoMock.Verify(r => r.Cadastrar(It.IsAny<Produto>()), Times.Never);
    }

    [TestMethod]
    public void Deve_RejeitarCadastro_ComNomeDuplicado_CaseInsensitive()
    {
        // Arrange
        CadastrarProdutoDto dto = new("cerveja premium", 8.50m);

        repositorioProdutoMock.Setup(r => r.SelecionarTodos())
            .Returns([new Produto("CERVEJA PREMIUM", 8.50m)]);

        // Act
        Result resultado = servicoProduto.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Já existe um produto com este nome.", resultado.Errors[0].Message);
        repositorioProdutoMock.Verify(r => r.Cadastrar(It.IsAny<Produto>()), Times.Never);
    }

    [TestMethod]
    public void Deve_RejeitarCadastro_ComNomeVazio()
    {
        // Arrange
        CadastrarProdutoDto dto = new("", 8.50m);

        repositorioProdutoMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        Result resultado = servicoProduto.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("O campo \"Nome\" é obrigatório.", resultado.Errors[0].Message);
        repositorioProdutoMock.Verify(r => r.Cadastrar(It.IsAny<Produto>()), Times.Never);
    }

    [TestMethod]
    public void Deve_RejeitarCadastro_ComPrecoZero()
    {
        // Arrange
        CadastrarProdutoDto dto = new("Cerveja", 0m);

        repositorioProdutoMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        Result resultado = servicoProduto.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("O campo \"Preço\" deve ser maior que zero.", resultado.Errors[0].Message);
        repositorioProdutoMock.Verify(r => r.Cadastrar(It.IsAny<Produto>()), Times.Never);
    }

    [TestMethod]
    public void Deve_RejeitarCadastro_ComPrecoNegativo()
    {
        // Arrange
        CadastrarProdutoDto dto = new("Cerveja", -5.00m);

        repositorioProdutoMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        Result resultado = servicoProduto.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("O campo \"Preço\" deve ser maior que zero.", resultado.Errors[0].Message);
        repositorioProdutoMock.Verify(r => r.Cadastrar(It.IsAny<Produto>()), Times.Never);
    }

    [TestMethod]
    public void Deve_EditarProduto_ComDadosValidos()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        EditarProdutoDto dto = new(id, "Refrigerante", 5.00m);

        repositorioProdutoMock.Setup(r => r.SelecionarTodos()).Returns([]);
        repositorioProdutoMock.Setup(r => r.Editar(id, It.IsAny<Produto>())).Returns(true);

        // Act
        Result resultado = servicoProduto.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioProdutoMock.Verify(r => r.Editar(id, It.Is<Produto>(p =>
            p.Nome == "Refrigerante" && p.Preco == 5.00m
        )), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarEdicao_ComNomeDuplicado()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        Guid outroId = Guid.CreateVersion7();
        EditarProdutoDto dto = new(id, "Cerveja Premium", 8.50m);

        var outroProduto = new Produto("Cerveja Premium", 8.50m) { Id = outroId };

        repositorioProdutoMock.Setup(r => r.SelecionarTodos())
            .Returns([outroProduto]);

        // Act
        Result resultado = servicoProduto.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Já existe um produto com este nome.", resultado.Errors[0].Message);
        repositorioProdutoMock.Verify(r => r.Editar(It.IsAny<Guid>(), It.IsAny<Produto>()), Times.Never);
    }

    [TestMethod]
    public void Deve_PermitirManteroNomoDuranteEdicao()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        var produtoExistente = new Produto("Cerveja Premium", 8.50m) { Id = id };
        EditarProdutoDto dto = new(id, "Cerveja Premium", 9.00m);

        repositorioProdutoMock.Setup(r => r.SelecionarTodos())
            .Returns([produtoExistente]);
        repositorioProdutoMock.Setup(r => r.Editar(id, It.IsAny<Produto>())).Returns(true);

        // Act
        Result resultado = servicoProduto.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioProdutoMock.Verify(r => r.Editar(id, It.IsAny<Produto>()), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarEdicao_QuandoProdutoNaoExistir()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        EditarProdutoDto dto = new(id, "Cerveja", 8.50m);

        repositorioProdutoMock.Setup(r => r.SelecionarTodos()).Returns([]);
        repositorioProdutoMock.Setup(r => r.Editar(id, It.IsAny<Produto>())).Returns(false);

        // Act
        Result resultado = servicoProduto.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Produto não encontrado.", resultado.Errors[0].Message);
    }

    [TestMethod]
    public void Deve_ExcluirProduto_SemVinculo()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        var produto = new Produto("Cerveja", 8.50m) { Id = id };

        repositorioProdutoMock.Setup(r => r.SelecionarPorId(id)).Returns(produto);
        repositorioProdutoMock.Setup(r => r.Excluir(id)).Returns(true);

        // Act
        Result resultado = servicoProduto.Excluir(id);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioProdutoMock.Verify(r => r.Excluir(id), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarExclusao_QuandoProdutoNaoExistir()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();

        repositorioProdutoMock.Setup(r => r.SelecionarPorId(id)).Returns((Produto?)null);

        // Act
        Result resultado = servicoProduto.Excluir(id);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Produto não encontrado.", resultado.Errors[0].Message);
        repositorioProdutoMock.Verify(r => r.Excluir(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public void Deve_SelecionarTodos()
    {
        // Arrange
        var produtos = new List<Produto>
        {
            new("Cerveja", 8.50m) { Id = Guid.CreateVersion7() },
            new("Refrigerante", 5.00m) { Id = Guid.CreateVersion7() }
        };

        repositorioProdutoMock.Setup(r => r.SelecionarTodos()).Returns(produtos);

        // Act
        List<ListarProdutoDto> resultado = servicoProduto.SelecionarTodos();

        // Assert
        Assert.AreEqual(2, resultado.Count);
        Assert.AreEqual("Cerveja", resultado[0].Nome);
        Assert.AreEqual("Refrigerante", resultado[1].Nome);
    }

    [TestMethod]
    public void Deve_BuscarProduto_PorNome()
    {
        // Arrange
        var produtos = new List<Produto>
        {
            new("Cerveja Premium", 8.50m) { Id = Guid.CreateVersion7() },
            new("Cerveja Artesanal", 10.00m) { Id = Guid.CreateVersion7() },
            new("Refrigerante", 5.00m) { Id = Guid.CreateVersion7() }
        };

        repositorioProdutoMock.Setup(r => r.SelecionarTodos()).Returns(produtos);

        // Act
        List<ListarProdutoDto> resultado = servicoProduto.Buscar("Cerveja");

        // Assert
        Assert.AreEqual(2, resultado.Count);
        Assert.IsTrue(resultado.All(p => p.Nome.Contains("Cerveja")));
    }

    [TestMethod]
    public void Deve_BuscarProduto_CaseInsensitive()
    {
        // Arrange
        var produtos = new List<Produto>
        {
            new("Cerveja Premium", 8.50m) { Id = Guid.CreateVersion7() },
            new("Refrigerante", 5.00m) { Id = Guid.CreateVersion7() }
        };

        repositorioProdutoMock.Setup(r => r.SelecionarTodos()).Returns(produtos);

        // Act
        List<ListarProdutoDto> resultado = servicoProduto.Buscar("cerveja");

        // Assert
        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("Cerveja Premium", resultado[0].Nome);
    }

    [TestMethod]
    public void Deve_RetornarTodos_QuandoBuscaForVazia()
    {
        // Arrange
        var produtos = new List<Produto>
        {
            new("Cerveja", 8.50m) { Id = Guid.CreateVersion7() },
            new("Refrigerante", 5.00m) { Id = Guid.CreateVersion7() }
        };

        repositorioProdutoMock.Setup(r => r.SelecionarTodos()).Returns(produtos);

        // Act
        List<ListarProdutoDto> resultado = servicoProduto.Buscar(null);

        // Assert
        Assert.AreEqual(2, resultado.Count);
    }

    [TestMethod]
    public void Deve_SelecionarPorId()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        var produto = new Produto("Cerveja", 8.50m) { Id = id };

        repositorioProdutoMock.Setup(r => r.SelecionarPorId(id)).Returns(produto);

        // Act
        var resultado = servicoProduto.SelecionarPorId(id);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual("Cerveja", resultado.Value.Nome);
        Assert.AreEqual(8.50m, resultado.Value.Preco);
    }

    [TestMethod]
    public void Deve_RetornarErro_AoSelecionarPorIdInexistente()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();

        repositorioProdutoMock.Setup(r => r.SelecionarPorId(id)).Returns((Produto?)null);

        // Act
        var resultado = servicoProduto.SelecionarPorId(id);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Produto não encontrado.", resultado.Errors[0].Message);
    }
}
