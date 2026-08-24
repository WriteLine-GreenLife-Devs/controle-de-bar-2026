using ControleDeBar.Dominio.Compartilhado.Identity;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using ControleDeBar.Dominio.Modulos.ModuloPedido;
using ControleDeBar.Dominio.Modulos.ModuloProduto;
using ControleDeBar.Infra.Compartilhado.Orm;
using ControleDeBar.Infra.Modulos.ModuloConta;
using ControleDeBar.Infra.Modulos.ModuloGarcom;
using ControleDeBar.Infra.Modulos.ModuloMesa;
using ControleDeBar.Infra.Modulos.ModuloPedido;
using ControleDeBar.Infra.Modulos.ModuloProduto;
using ControleDeBar.Testes.Integracao.Compartilhado.Identity;
using ControleDeBar.Testes.Integracao.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ControleDeBar.Testes.Integracao.ModuloPedido;

[TestClass]
public sealed class RepositorioPedidoEmOrmTests : RepositorioBaseEmOrmTests
{
    private RepositorioPedidoEmOrm repositorioPedido = null!;
    private RepositorioContaEmOrm repositorioConta = null!;
    private RepositorioProdutoEmOrm repositorioProduto = null!;

    [TestInitialize]
    public override void InicializarContexto()
    {
        base.InicializarContexto();
        repositorioPedido = new RepositorioPedidoEmOrm(dbContext);
        repositorioConta = new RepositorioContaEmOrm(dbContext);
        repositorioProduto = new RepositorioProdutoEmOrm(dbContext);
    }

    [TestMethod]
    public void CadastrarESelecionarPorId_CarregaRegistro()
    {
        // Arrange
        Produto produto = new("Cerveja Premium", 8.50m);
        repositorioProduto.Cadastrar(produto);

        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Carlos");
        repositorioConta.Cadastrar(conta);

        Pedido pedido = new(conta.Id, produto.Id, produto.Nome, produto.Preco, 2);

        // Act
        repositorioPedido.Cadastrar(pedido);
        dbContext.ChangeTracker.Clear();

        Pedido? pedidoSelecionado = repositorioPedido.SelecionarPorId(pedido.Id);

        // Assert
        Assert.IsNotNull(pedidoSelecionado);
        Assert.AreEqual("Cerveja Premium", pedidoSelecionado.NomeProduto);
        Assert.AreEqual(8.50m, pedidoSelecionado.PrecoPraticado);
        Assert.AreEqual(2, pedidoSelecionado.Quantidade);
    }

    [TestMethod]
    public void Cadastrar_PreencheUserIdDoUsuarioAutenticado()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);
        repositorioProduto.Cadastrar(produto);

        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Carlos");
        repositorioConta.Cadastrar(conta);

        Pedido pedido = new(conta.Id, produto.Id, "Cerveja", 8.50m, 1);

        // Act
        repositorioPedido.Cadastrar(pedido);
        dbContext.ChangeTracker.Clear();

        Pedido? pedidoSelecionado = repositorioPedido.SelecionarPorId(pedido.Id);

        // Assert
        Assert.IsNotNull(pedidoSelecionado);
        Assert.AreEqual(userId, pedidoSelecionado.UserId);
    }

    [TestMethod]
    public void SelecionarTodos_CarregaSomenteRegistrosDoUsuarioAutenticado()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);
        repositorioProduto.Cadastrar(produto);

        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Carlos");
        repositorioConta.Cadastrar(conta);

        repositorioPedido.Cadastrar(new Pedido(conta.Id, produto.Id, "Cerveja", 8.50m, 1));
        repositorioPedido.Cadastrar(new Pedido(conta.Id, produto.Id, "Cerveja", 8.50m, 2));

        Guid outroUsuarioId = Guid.CreateVersion7();

        using (ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId))
        {
            RepositorioPedidoEmOrm outroRepositorio = new(outroContexto);

            // Act
            List<Pedido> pedidosUsuarioAtual = repositorioPedido.SelecionarTodos();
            List<Pedido> pedidosOutroUsuario = outroRepositorio.SelecionarTodos();

            // Assert
            Assert.AreEqual(2, pedidosUsuarioAtual.Count);
            Assert.AreEqual(0, pedidosOutroUsuario.Count);
        }
    }

    [TestMethod]
    public void SelecionarPorId_RetornaNuloParaPedidoDeOutroUsuario()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);
        repositorioProduto.Cadastrar(produto);

        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Carlos");
        repositorioConta.Cadastrar(conta);

        Pedido pedido = new(conta.Id, produto.Id, "Cerveja", 8.50m, 1);
        repositorioPedido.Cadastrar(pedido);

        Guid outroUsuarioId = Guid.CreateVersion7();

        using (ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId))
        {
            RepositorioPedidoEmOrm outroRepositorio = new(outroContexto);

            // Act
            Pedido? pedidoSelecionado = outroRepositorio.SelecionarPorId(pedido.Id);

            // Assert
            Assert.IsNull(pedidoSelecionado);
        }
    }

    [TestMethod]
    public void Excluir_RemoveRegistroExistente()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);
        repositorioProduto.Cadastrar(produto);

        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Carlos");
        repositorioConta.Cadastrar(conta);

        Pedido pedido = new(conta.Id, produto.Id, "Cerveja", 8.50m, 1);
        repositorioPedido.Cadastrar(pedido);

        // Act
        bool conseguiuExcluir = repositorioPedido.Excluir(pedido.Id);
        dbContext.ChangeTracker.Clear();

        // Assert
        Assert.IsTrue(conseguiuExcluir);
        Assert.IsNull(repositorioPedido.SelecionarPorId(pedido.Id));
    }

    [TestMethod]
    public void Excluir_RetornaFalsoParaPedidoInexistente()
    {
        // Arrange
        Guid idInexistente = Guid.CreateVersion7();

        // Act
        bool resultado = repositorioPedido.Excluir(idInexistente);

        // Assert
        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void Deve_Filtrar_MultiplosPedidosDamesmaConta()
    {
        // Arrange
        Produto produto1 = new("Cerveja", 8.50m);
        Produto produto2 = new("Refrigerante", 5.00m);
        repositorioProduto.Cadastrar(produto1);
        repositorioProduto.Cadastrar(produto2);

        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Carlos");
        repositorioConta.Cadastrar(conta);

        Pedido pedido1 = new(conta.Id, produto1.Id, "Cerveja", 8.50m, 1);
        Pedido pedido2 = new(conta.Id, produto2.Id, "Refrigerante", 5.00m, 2);
        repositorioPedido.Cadastrar(pedido1);
        repositorioPedido.Cadastrar(pedido2);

        dbContext.ChangeTracker.Clear();

        // Act
        List<Pedido> resultado = repositorioPedido.Filtrar(p => p.ContaId == conta.Id);

        // Assert
        Assert.AreEqual(2, resultado.Count);
        Assert.IsTrue(resultado.All(p => p.ContaId == conta.Id));
    }

    [TestMethod]
    public void Deve_MantorSnapshot_AoCarregarDoBanco()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);
        repositorioProduto.Cadastrar(produto);

        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Carlos");
        repositorioConta.Cadastrar(conta);

        Pedido pedido = new(conta.Id, produto.Id, "Cerveja Original", 8.50m, 2);
        repositorioPedido.Cadastrar(pedido);

        dbContext.ChangeTracker.Clear();

        // Act
        Pedido? pedidoCarregado = repositorioPedido.SelecionarPorId(pedido.Id);

        // Assert
        Assert.IsNotNull(pedidoCarregado);
        Assert.AreEqual("Cerveja Original", pedidoCarregado.NomeProduto);
        Assert.AreEqual(8.50m, pedidoCarregado.PrecoPraticado);
    }

    [TestMethod]
    public void Deve_CalcularSubtotalCorretamente_Apos_Recarregar()
    {
        // Arrange
        Produto produto = new("Cerveja", 10.00m);
        repositorioProduto.Cadastrar(produto);

        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Carlos");
        repositorioConta.Cadastrar(conta);

        Pedido pedido = new(conta.Id, produto.Id, "Cerveja", 10.00m, 3);
        repositorioPedido.Cadastrar(pedido);

        dbContext.ChangeTracker.Clear();

        // Act
        Pedido? pedidoCarregado = repositorioPedido.SelecionarPorId(pedido.Id);

        // Assert
        Assert.IsNotNull(pedidoCarregado);
        Assert.AreEqual(30.00m, pedidoCarregado.Subtotal);
    }

    [TestMethod]
    public void Deve_Permitir_MesmoProdutoEmMultiplosPedidos()
    {
        // Arrange
        Produto produto = new("Cerveja", 8.50m);
        repositorioProduto.Cadastrar(produto);

        Conta conta1 = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Carlos");
        Conta conta2 = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Maria");
        repositorioConta.Cadastrar(conta1);
        repositorioConta.Cadastrar(conta2);

        Pedido pedido1 = new(conta1.Id, produto.Id, "Cerveja", 8.50m, 1);
        Pedido pedido2 = new(conta2.Id, produto.Id, "Cerveja", 8.50m, 2);
        repositorioPedido.Cadastrar(pedido1);
        repositorioPedido.Cadastrar(pedido2);

        // Act
        List<Pedido> todosPedidos = repositorioPedido.SelecionarTodos();

        // Assert
        Assert.AreEqual(2, todosPedidos.Count);
        Assert.AreEqual(2, todosPedidos.Count(p => p.ProdutoId == produto.Id));
    }

    [TestMethod]
    public void Deve_PreservarSnapshot_DoProduto_AposAlteracaoDoProduto()
    {
        // Arrange
        Produto produto = new("Coca-Cola", 8m);
        repositorioProduto.Cadastrar(produto);

        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "João");
        repositorioConta.Cadastrar(conta);

        Pedido pedido = new(conta.Id, produto.Id, produto.Nome, produto.Preco, 2);
        repositorioPedido.Cadastrar(pedido);

        // Verificar valores iniciais
        dbContext.ChangeTracker.Clear();
        Pedido? pedidoAntes = repositorioPedido.SelecionarPorId(pedido.Id);

        Assert.IsNotNull(pedidoAntes);
        Assert.AreEqual("Coca-Cola", pedidoAntes.NomeProduto);
        Assert.AreEqual(8m, pedidoAntes.PrecoPraticado);
        Assert.AreEqual(2, pedidoAntes.Quantidade);
        Assert.AreEqual(16m, pedidoAntes.Subtotal);

        // Act - Alterar o Produto
        Produto produtoAtualizado = new("Coca-Cola 600ml", 12m);
        repositorioProduto.Editar(produto.Id, produtoAtualizado);
        dbContext.ChangeTracker.Clear();

        // Assert - Verificar que Pedido mantém snapshot original
        Pedido? pedidoDepois = repositorioPedido.SelecionarPorId(pedido.Id);

        Assert.IsNotNull(pedidoDepois);
        Assert.AreEqual("Coca-Cola", pedidoDepois.NomeProduto, "Nome do Pedido deve manter snapshot original");
        Assert.AreEqual(8m, pedidoDepois.PrecoPraticado, "Preço do Pedido deve manter snapshot original");
        Assert.AreEqual(2, pedidoDepois.Quantidade, "Quantidade não deve ter mudado");
        Assert.AreEqual(16m, pedidoDepois.Subtotal, "Subtotal deve permanecer igual (2 x 8)");

        // Verificar que Produto foi alterado
        Produto? produtoAlterado = repositorioProduto.SelecionarPorId(produto.Id);
        Assert.IsNotNull(produtoAlterado);
        Assert.AreEqual("Coca-Cola 600ml", produtoAlterado.Nome, "Produto deve ter nome alterado");
        Assert.AreEqual(12m, produtoAlterado.Preco, "Produto deve ter preço alterado");
    }
}
