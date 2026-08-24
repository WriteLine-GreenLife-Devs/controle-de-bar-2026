using ControleDeBar.Dominio.Modulos.ModuloPedido;

namespace ControleDeBar.Testes.Unidade.Modulos.ModuloPedido;

[TestClass]
public sealed class PedidoTests
{
    [TestMethod]
    public void Deve_CriarPedido_Valido()
    {
        // Arrange e Act
        Guid contaId = Guid.CreateVersion7();
        Guid produtoId = Guid.CreateVersion7();
        Pedido pedido = new(contaId, produtoId, "Cerveja Premium", 8.50m, 2);

        // Assert
        Assert.AreEqual(contaId, pedido.ContaId);
        Assert.AreEqual(produtoId, pedido.ProdutoId);
        Assert.AreEqual("Cerveja Premium", pedido.NomeProduto);
        Assert.AreEqual(8.50m, pedido.PrecoPraticado);
        Assert.AreEqual(2, pedido.Quantidade);
        Assert.AreEqual(17.00m, pedido.Subtotal);
        Assert.IsEmpty(pedido.Validar());
    }

    [TestMethod]
    public void Deve_RejeitarPedido_ComContaIdVazio()
    {
        // Arrange
        Pedido pedido = new(Guid.Empty, Guid.CreateVersion7(), "Cerveja", 8.50m, 1);

        // Act
        List<string> erros = pedido.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Conta\" é obrigatório.");
    }

    [TestMethod]
    public void Deve_RejeitarPedido_ComProdutoIdVazio()
    {
        // Arrange
        Pedido pedido = new(Guid.CreateVersion7(), Guid.Empty, "Cerveja", 8.50m, 1);

        // Act
        List<string> erros = pedido.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Produto\" é obrigatório.");
    }

    [TestMethod]
    public void Deve_RejeitarPedido_ComNomeProdutoVazio()
    {
        // Arrange
        Pedido pedido = new(Guid.CreateVersion7(), Guid.CreateVersion7(), string.Empty, 8.50m, 1);

        // Act
        List<string> erros = pedido.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O nome do produto é obrigatório.");
    }

    [TestMethod]
    public void Deve_RejeitarPedido_ComPrecoPraticadoZero()
    {
        // Arrange
        Pedido pedido = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Cerveja", 0m, 1);

        // Act
        List<string> erros = pedido.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O preço praticado deve ser maior que zero.");
    }

    [TestMethod]
    public void Deve_RejeitarPedido_ComPrecoPraticadoNegativo()
    {
        // Arrange
        Pedido pedido = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Cerveja", -8.50m, 1);

        // Act
        List<string> erros = pedido.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O preço praticado deve ser maior que zero.");
    }

    [TestMethod]
    public void Deve_RejeitarPedido_ComQuantidadeZero()
    {
        // Arrange
        Pedido pedido = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Cerveja", 8.50m, 0);

        // Act
        List<string> erros = pedido.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Quantidade\" deve ser maior que zero.");
    }

    [TestMethod]
    public void Deve_RejeitarPedido_ComQuantidadeNegativa()
    {
        // Arrange
        Pedido pedido = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Cerveja", 8.50m, -5);

        // Act
        List<string> erros = pedido.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Quantidade\" deve ser maior que zero.");
    }

    [TestMethod]
    public void Deve_TrimNomeProdutoAoCriar()
    {
        // Arrange e Act
        Pedido pedido = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "  Cerveja Premium  ", 8.50m, 1);

        // Assert
        Assert.AreEqual("Cerveja Premium", pedido.NomeProduto);
    }

    [TestMethod]
    public void Deve_CalcularSubtotalCorretamente()
    {
        // Arrange
        decimal preco = 10.50m;
        int quantidade = 3;
        Pedido pedido = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Refrigerante", preco, quantidade);

        // Act
        decimal subtotal = pedido.Subtotal;

        // Assert
        Assert.AreEqual(31.50m, subtotal);
    }

    [TestMethod]
    public void Deve_AtualizarPedido()
    {
        // Arrange
        Pedido pedido = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Cerveja", 8.50m, 1);
        Pedido pedidoAtualizado = new(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Refrigerante",
            5.00m,
            2
        );

        // Act
        pedido.Atualizar(pedidoAtualizado);

        // Assert
        Assert.AreEqual("Refrigerante", pedido.NomeProduto);
        Assert.AreEqual(5.00m, pedido.PrecoPraticado);
        Assert.AreEqual(2, pedido.Quantidade);
    }
}
