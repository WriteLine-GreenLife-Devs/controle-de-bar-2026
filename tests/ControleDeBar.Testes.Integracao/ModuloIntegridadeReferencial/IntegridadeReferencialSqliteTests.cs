using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using ControleDeBar.Dominio.Modulos.ModuloPedido;
using ControleDeBar.Dominio.Modulos.ModuloProduto;
using ControleDeBar.Testes.Integracao.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;

namespace ControleDeBar.Testes.Integracao.ModuloIntegridadeReferencial;

[TestClass]
public sealed class IntegridadeReferencialSqliteTests : SqliteIntegrationTestBase
{
    [TestMethod]
    public void Deve_ImpedirExclusao_DeMesaVinculadaAConta()
    {
        // Arrange
        (Mesa mesa, _, Conta conta) = PersistirConta();
        dbContext.ChangeTracker.Clear();

        // Act
        dbContext.Mesas.Remove(mesa);

        // Assert
        Assert.Throws<DbUpdateException>(() => dbContext.SaveChanges());
        dbContext.ChangeTracker.Clear();

        Assert.IsNotNull(dbContext.Mesas.SingleOrDefault(m => m.Id == mesa.Id));
        Assert.IsNotNull(dbContext.Contas.SingleOrDefault(c => c.Id == conta.Id));
    }

    [TestMethod]
    public void Deve_ImpedirExclusao_DeGarcomVinculadoAConta()
    {
        // Arrange
        (_, Garcom garcom, Conta conta) = PersistirConta();
        dbContext.ChangeTracker.Clear();

        // Act
        dbContext.Garcons.Remove(garcom);

        // Assert
        Assert.Throws<DbUpdateException>(() => dbContext.SaveChanges());
        dbContext.ChangeTracker.Clear();

        Assert.IsNotNull(dbContext.Garcons.SingleOrDefault(g => g.Id == garcom.Id));
        Assert.IsNotNull(dbContext.Contas.SingleOrDefault(c => c.Id == conta.Id));
    }

    [TestMethod]
    public void Deve_ImpedirExclusao_DeProdutoVinculadoAPedido()
    {
        // Arrange
        (Conta conta, Produto produto, Pedido pedido) = PersistirPedido();
        dbContext.ChangeTracker.Clear();

        // Act
        dbContext.Produtos.Remove(produto);

        // Assert
        Assert.Throws<DbUpdateException>(() => dbContext.SaveChanges());
        dbContext.ChangeTracker.Clear();

        Assert.IsNotNull(dbContext.Produtos.SingleOrDefault(p => p.Id == produto.Id));
        Assert.IsNotNull(dbContext.Pedidos.SingleOrDefault(p => p.Id == pedido.Id));
        Assert.IsNotNull(dbContext.Contas.SingleOrDefault(c => c.Id == conta.Id));
    }

    [TestMethod]
    public void Deve_ImpedirExclusao_DeContaVinculadaAPedido()
    {
        // Arrange
        (Conta conta, Produto _, Pedido pedido) = PersistirPedido();
        dbContext.ChangeTracker.Clear();

        // Act
        dbContext.Contas.Remove(conta);

        // Assert
        Assert.Throws<DbUpdateException>(() => dbContext.SaveChanges());
        dbContext.ChangeTracker.Clear();

        Assert.IsNotNull(dbContext.Contas.SingleOrDefault(c => c.Id == conta.Id));
        Assert.IsNotNull(dbContext.Pedidos.SingleOrDefault(p => p.Id == pedido.Id));
    }

    private (Mesa Mesa, Garcom Garcom, Conta Conta) PersistirConta()
    {
        Mesa mesa = new(1, 4);
        Garcom garcom = new("Marcos");
        dbContext.Mesas.Add(mesa);
        dbContext.Garcons.Add(garcom);
        dbContext.SaveChanges();

        Conta conta = new(mesa.Id, garcom.Id, "Carlos");
        dbContext.Contas.Add(conta);
        dbContext.SaveChanges();

        return (mesa, garcom, conta);
    }

    private (Conta Conta, Produto Produto, Pedido Pedido) PersistirPedido()
    {
        (Mesa mesa, Garcom garcom, Conta conta) = PersistirConta();
        Produto produto = new("Cerveja", 8.50m);
        dbContext.Produtos.Add(produto);
        dbContext.SaveChanges();

        Pedido pedido = new(conta.Id, produto.Id, produto.Nome, produto.Preco, 2);
        dbContext.Pedidos.Add(pedido);
        dbContext.SaveChanges();

        return (conta, produto, pedido);
    }
}
