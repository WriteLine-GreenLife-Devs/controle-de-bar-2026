using ControleDeBar.Dominio.Modulos.ModuloProduto;

namespace ControleDeBar.Testes.Unidade.Modulos.ModuloProduto;

[TestClass]
public sealed class ProdutoTests
{
    [TestMethod]
    public void Deve_CriarProduto_ComDadosValidos()
    {
        // Arrange e Act
        Produto produto = new("Cerveja Premium", 8.50m);

        // Assert
        Assert.AreEqual("Cerveja Premium", produto.Nome);
        Assert.AreEqual(8.50m, produto.Preco);
        Assert.IsEmpty(produto.Validar());
    }

    [TestMethod]
    public void Deve_RejeitarProduto_ComNomeVazio()
    {
        // Arrange
        Produto produto = new(string.Empty, 8.50m);

        // Act
        List<string> erros = produto.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Nome\" é obrigatório.");
    }

    [TestMethod]
    public void Deve_RejeitarProduto_ComNomeNulo()
    {
        // Arrange
        Produto produto = new(null, 8.50m);

        // Act
        List<string> erros = produto.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Nome\" é obrigatório.");
    }

    [TestMethod]
    public void Deve_RejeitarProduto_ComNomeApenasEspacos()
    {
        // Arrange
        Produto produto = new("   ", 8.50m);

        // Act
        List<string> erros = produto.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Nome\" é obrigatório.");
    }

    [TestMethod]
    public void Deve_RejeitarProduto_ComPrecoZero()
    {
        // Arrange
        Produto produto = new("Cerveja", 0m);

        // Act
        List<string> erros = produto.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Preço\" deve ser maior que zero.");
    }

    [TestMethod]
    public void Deve_RejeitarProduto_ComPrecoNegativo()
    {
        // Arrange
        Produto produto = new("Cerveja", -10.50m);

        // Act
        List<string> erros = produto.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Preço\" deve ser maior que zero.");
    }

    [TestMethod]
    public void Deve_TrimNomeAoTrimizar()
    {
        // Arrange
        Produto produto = new("  Cerveja Premium  ", 8.50m);

        // Act e Assert
        Assert.AreEqual("Cerveja Premium", produto.Nome);
    }

    [TestMethod]
    public void Deve_AtualizarProduto_ComNovosDados()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);
        Produto produtoAtualizado = new("Refrigerante", 5.00m);

        // Act
        produto.Atualizar(produtoAtualizado);

        // Assert
        Assert.AreEqual("Refrigerante", produto.Nome);
        Assert.AreEqual(5.00m, produto.Preco);
    }

    [TestMethod]
    public void Deve_TrimNomeAoAtualizar()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);
        Produto produtoAtualizado = new("  Refrigerante  ", 5.00m);

        // Act
        produto.Atualizar(produtoAtualizado);

        // Assert
        Assert.AreEqual("Refrigerante", produto.Nome);
    }
}
