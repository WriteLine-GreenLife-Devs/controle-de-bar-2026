using ControleDeBar.Aplicacao.Modulos.ModuloPedido;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloPedido;
using ControleDeBar.Dominio.Modulos.ModuloProduto;
using FluentResults;
using Moq;

namespace ControleDeBar.Testes.Unidade.Modulos.ModuloPedido;

[TestClass]
public sealed class ServicoPedidoTests
{
    private Mock<IRepositorioPedido> repositorioPedidoMock = null!;
    private Mock<IRepositorioConta> repositorioContaMock = null!;
    private Mock<IRepositorioProduto> repositorioProdutoMock = null!;
    private ServicoPedido servicoPedido = null!;

    [TestInitialize]
    public void Inicializar()
    {
        repositorioPedidoMock = new Mock<IRepositorioPedido>();
        repositorioContaMock = new Mock<IRepositorioConta>();
        repositorioProdutoMock = new Mock<IRepositorioProduto>();
        servicoPedido = new ServicoPedido(
            repositorioPedidoMock.Object,
            repositorioContaMock.Object,
            repositorioProdutoMock.Object
        );
    }

    [TestMethod]
    public void Deve_AdicionarPedido_ComDadosValidos()
    {
        // Arrange
        Guid contaId = Guid.CreateVersion7();
        Guid produtoId = Guid.CreateVersion7();
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();

        Conta conta = new(mesaId, garcomId, "Carlos") { Id = contaId, Status = StatusConta.Aberta };
        Produto produto = new("Cerveja Premium", 8.50m) { Id = produtoId };
        AdicionarPedidoDto dto = new(contaId, produtoId, 2);

        repositorioContaMock.Setup(r => r.SelecionarPorId(contaId)).Returns(conta);
        repositorioProdutoMock.Setup(r => r.SelecionarPorId(produtoId)).Returns(produto);

        // Act
        Result resultado = servicoPedido.Adicionar(dto);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioPedidoMock.Verify(r => r.Cadastrar(It.Is<Pedido>(p =>
            p.ContaId == contaId &&
            p.ProdutoId == produtoId &&
            p.NomeProduto == "Cerveja Premium" &&
            p.PrecoPraticado == 8.50m &&
            p.Quantidade == 2
        )), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarAdicao_QuandoContaNaoExistir()
    {
        // Arrange
        Guid contaId = Guid.CreateVersion7();
        Guid produtoId = Guid.CreateVersion7();
        AdicionarPedidoDto dto = new(contaId, produtoId, 1);

        repositorioContaMock.Setup(r => r.SelecionarPorId(contaId)).Returns((Conta?)null);

        // Act
        Result resultado = servicoPedido.Adicionar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Conta não encontrada.", resultado.Errors[0].Message);
        repositorioPedidoMock.Verify(r => r.Cadastrar(It.IsAny<Pedido>()), Times.Never);
    }

    [TestMethod]
    public void Deve_RejeitarAdicao_QuandoContaEstFechada()
    {
        // Arrange
        Guid contaId = Guid.CreateVersion7();
        Guid produtoId = Guid.CreateVersion7();
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();

        Conta conta = new(mesaId, garcomId, "Carlos") { Id = contaId };
        conta.Fechar();

        Produto produto = new("Cerveja", 8.50m) { Id = produtoId };
        AdicionarPedidoDto dto = new(contaId, produtoId, 1);

        repositorioContaMock.Setup(r => r.SelecionarPorId(contaId)).Returns(conta);
        repositorioProdutoMock.Setup(r => r.SelecionarPorId(produtoId)).Returns(produto);

        // Act
        Result resultado = servicoPedido.Adicionar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Não é possível adicionar pedidos a uma conta fechada.", resultado.Errors[0].Message);
        repositorioPedidoMock.Verify(r => r.Cadastrar(It.IsAny<Pedido>()), Times.Never);
    }

    [TestMethod]
    public void Deve_RejeitarAdicao_QuandoProdutoNaoExistir()
    {
        // Arrange
        Guid contaId = Guid.CreateVersion7();
        Guid produtoId = Guid.CreateVersion7();
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();

        Conta conta = new(mesaId, garcomId, "Carlos") { Id = contaId, Status = StatusConta.Aberta };
        AdicionarPedidoDto dto = new(contaId, produtoId, 1);

        repositorioContaMock.Setup(r => r.SelecionarPorId(contaId)).Returns(conta);
        repositorioProdutoMock.Setup(r => r.SelecionarPorId(produtoId)).Returns((Produto?)null);

        // Act
        Result resultado = servicoPedido.Adicionar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Produto não encontrado.", resultado.Errors[0].Message);
        repositorioPedidoMock.Verify(r => r.Cadastrar(It.IsAny<Pedido>()), Times.Never);
    }

    [TestMethod]
    public void Deve_CriarSnapshot_ComNomeProduto()
    {
        // Arrange
        Guid contaId = Guid.CreateVersion7();
        Guid produtoId = Guid.CreateVersion7();
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();

        Conta conta = new(mesaId, garcomId, "Carlos") { Id = contaId, Status = StatusConta.Aberta };
        Produto produto = new("Cerveja Premium", 8.50m) { Id = produtoId };
        AdicionarPedidoDto dto = new(contaId, produtoId, 1);

        repositorioContaMock.Setup(r => r.SelecionarPorId(contaId)).Returns(conta);
        repositorioProdutoMock.Setup(r => r.SelecionarPorId(produtoId)).Returns(produto);

        // Act
        Result resultado = servicoPedido.Adicionar(dto);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioPedidoMock.Verify(r => r.Cadastrar(It.Is<Pedido>(p =>
            p.NomeProduto == "Cerveja Premium"
        )), Times.Once);
    }

    [TestMethod]
    public void Deve_CriarSnapshot_ComPrecoProduto()
    {
        // Arrange
        Guid contaId = Guid.CreateVersion7();
        Guid produtoId = Guid.CreateVersion7();
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();

        Conta conta = new(mesaId, garcomId, "Carlos") { Id = contaId, Status = StatusConta.Aberta };
        Produto produto = new("Cerveja", 8.50m) { Id = produtoId };
        AdicionarPedidoDto dto = new(contaId, produtoId, 2);

        repositorioContaMock.Setup(r => r.SelecionarPorId(contaId)).Returns(conta);
        repositorioProdutoMock.Setup(r => r.SelecionarPorId(produtoId)).Returns(produto);

        // Act
        Result resultado = servicoPedido.Adicionar(dto);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioPedidoMock.Verify(r => r.Cadastrar(It.Is<Pedido>(p =>
            p.PrecoPraticado == 8.50m
        )), Times.Once);
    }

    [TestMethod]
    public void Deve_RemoverPedido_DeContaAberta()
    {
        // Arrange
        Guid pedidoId = Guid.CreateVersion7();
        Guid contaId = Guid.CreateVersion7();
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();

        Conta conta = new(mesaId, garcomId, "Carlos") { Id = contaId, Status = StatusConta.Aberta };
        Pedido pedido = new(contaId, Guid.CreateVersion7(), "Cerveja", 8.50m, 1) { Id = pedidoId };

        repositorioPedidoMock.Setup(r => r.SelecionarPorId(pedidoId)).Returns(pedido);
        repositorioContaMock.Setup(r => r.SelecionarPorId(contaId)).Returns(conta);

        // Act
        Result resultado = servicoPedido.Remover(pedidoId);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioPedidoMock.Verify(r => r.Excluir(pedidoId), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarRemocao_QuandoPedidoNaoExistir()
    {
        // Arrange
        Guid pedidoId = Guid.CreateVersion7();

        repositorioPedidoMock.Setup(r => r.SelecionarPorId(pedidoId)).Returns((Pedido?)null);

        // Act
        Result resultado = servicoPedido.Remover(pedidoId);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Pedido não encontrado.", resultado.Errors[0].Message);
        repositorioPedidoMock.Verify(r => r.Excluir(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public void Deve_RejeitarRemocao_DeContaFechada()
    {
        // Arrange
        Guid pedidoId = Guid.CreateVersion7();
        Guid contaId = Guid.CreateVersion7();
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();

        Conta conta = new(mesaId, garcomId, "Carlos") { Id = contaId };
        conta.Fechar();

        Pedido pedido = new(contaId, Guid.CreateVersion7(), "Cerveja", 8.50m, 1) { Id = pedidoId };

        repositorioPedidoMock.Setup(r => r.SelecionarPorId(pedidoId)).Returns(pedido);
        repositorioContaMock.Setup(r => r.SelecionarPorId(contaId)).Returns(conta);

        // Act
        Result resultado = servicoPedido.Remover(pedidoId);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Não é possível remover pedidos de uma conta fechada.", resultado.Errors[0].Message);
        repositorioPedidoMock.Verify(r => r.Excluir(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public void Deve_SelecionarPorId()
    {
        // Arrange
        Guid pedidoId = Guid.CreateVersion7();
        Pedido pedido = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Cerveja", 8.50m, 2) { Id = pedidoId };

        repositorioPedidoMock.Setup(r => r.SelecionarPorId(pedidoId)).Returns(pedido);

        // Act
        Result<ListarPedidoDto> resultado = servicoPedido.SelecionarPorId(pedidoId);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(pedidoId, resultado.Value.Id);
        Assert.AreEqual("Cerveja", resultado.Value.NomeProduto);
        Assert.AreEqual(8.50m, resultado.Value.PrecoPraticado);
        Assert.AreEqual(2, resultado.Value.Quantidade);
        Assert.AreEqual(17.00m, resultado.Value.Subtotal);
    }

    [TestMethod]
    public void Deve_SelecionarPorConta()
    {
        // Arrange
        Guid contaId = Guid.CreateVersion7();
        var pedidos = new List<Pedido>
        {
            new(contaId, Guid.CreateVersion7(), "Cerveja", 8.50m, 1) { Id = Guid.CreateVersion7() },
            new(contaId, Guid.CreateVersion7(), "Refrigerante", 5.00m, 2) { Id = Guid.CreateVersion7() }
        };

        repositorioPedidoMock.Setup(r => r.SelecionarTodos()).Returns(pedidos);

        // Act
        List<ListarPedidoDto> resultado = servicoPedido.SelecionarPorConta(contaId);

        // Assert
        Assert.AreEqual(2, resultado.Count);
        Assert.IsTrue(resultado.All(p => p.ContaId == contaId));
    }

    [TestMethod]
    public void Deve_CalcularTotal_SemPedidos()
    {
        // Arrange
        Guid contaId = Guid.CreateVersion7();

        repositorioPedidoMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        decimal total = servicoPedido.CalcularTotal(contaId);

        // Assert
        Assert.AreEqual(0m, total);
    }

    [TestMethod]
    public void Deve_CalcularTotal_ComUmPedido()
    {
        // Arrange
        Guid contaId = Guid.CreateVersion7();
        var pedidos = new List<Pedido>
        {
            new(contaId, Guid.CreateVersion7(), "Cerveja", 10.00m, 2) { Id = Guid.CreateVersion7() }
        };

        repositorioPedidoMock.Setup(r => r.SelecionarTodos()).Returns(pedidos);

        // Act
        decimal total = servicoPedido.CalcularTotal(contaId);

        // Assert
        Assert.AreEqual(20.00m, total);
    }

    [TestMethod]
    public void Deve_CalcularTotal_ComMultiplosPedidos()
    {
        // Arrange
        Guid contaId = Guid.CreateVersion7();
        var pedidos = new List<Pedido>
        {
            new(contaId, Guid.CreateVersion7(), "Cerveja", 10.00m, 2) { Id = Guid.CreateVersion7() },
            new(contaId, Guid.CreateVersion7(), "Refrigerante", 5.00m, 3) { Id = Guid.CreateVersion7() },
            new(contaId, Guid.CreateVersion7(), "Suco", 8.00m, 1) { Id = Guid.CreateVersion7() }
        };

        repositorioPedidoMock.Setup(r => r.SelecionarTodos()).Returns(pedidos);

        // Act
        decimal total = servicoPedido.CalcularTotal(contaId);

        // Assert
        Assert.AreEqual(43.00m, total); // (10*2) + (5*3) + (8*1) = 20 + 15 + 8 = 43
    }
}
