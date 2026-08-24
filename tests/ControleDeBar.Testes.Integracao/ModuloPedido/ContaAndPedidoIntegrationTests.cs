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
public sealed class ContaAndPedidoIntegrationTests : RepositorioBaseEmOrmTests
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
    public void Deve_PreservarPedidosETotal_AposFecharConta()
    {
        // Arrange
        Produto produto1 = new("Cerveja", 8.50m);
        Produto produto2 = new("Refrigerante", 5.00m);
        repositorioProduto.Cadastrar(produto1);
        repositorioProduto.Cadastrar(produto2);

        Mesa mesa = new(1, 4);
        Garcom garcom = new("Carlos");
        repositorioMesa.Cadastrar(mesa);
        repositorioGarcom.Cadastrar(garcom);

        Conta conta = new(mesa.Id, garcom.Id, "João Silva");
        repositorioConta.Cadastrar(conta);

        // Adicionar pedidos
        Pedido pedido1 = new(conta.Id, produto1.Id, produto1.Nome, produto1.Preco, 2);
        Pedido pedido2 = new(conta.Id, produto2.Id, produto2.Nome, produto2.Preco, 1);
        repositorioPedido.Cadastrar(pedido1);
        repositorioPedido.Cadastrar(pedido2);

        // Calcular total antes do fechamento
        decimal totalAntesDeFechamento = pedido1.Subtotal + pedido2.Subtotal;
        // (2 * 8.50) + (1 * 5.00) = 17.00 + 5.00 = 22.00

        int quantidadePedidosAntes = repositorioPedido.Filtrar(p => p.ContaId == conta.Id).Count;

        // Act - Fechar a Conta
        conta.Fechar();
        repositorioConta.Salvar();
        dbContext.ChangeTracker.Clear();

        // Assert - Verificar estado após fechamento
        Conta? contaBuscada = repositorioConta.SelecionarPorId(conta.Id);
        Assert.IsNotNull(contaBuscada);
        Assert.AreEqual(StatusConta.Fechada, contaBuscada.Status, "Conta deve estar Fechada");

        // Verificar que Pedidos continuam existentes
        List<Pedido> pedidosAposFechar = repositorioPedido.Filtrar(p => p.ContaId == conta.Id);
        Assert.AreEqual(quantidadePedidosAntes, pedidosAposFechar.Count, "Quantidade de pedidos deve ser a mesma");
        Assert.AreEqual(2, pedidosAposFechar.Count, "Deve ter exatamente 2 pedidos");

        // Verificar que NomeProduto e PrecoPraticado continuam iguais
        Pedido? pedido1Apos = repositorioPedido.SelecionarPorId(pedido1.Id);
        Pedido? pedido2Apos = repositorioPedido.SelecionarPorId(pedido2.Id);

        Assert.IsNotNull(pedido1Apos);
        Assert.IsNotNull(pedido2Apos);

        Assert.AreEqual("Cerveja", pedido1Apos.NomeProduto);
        Assert.AreEqual(8.50m, pedido1Apos.PrecoPraticado);
        Assert.AreEqual("Refrigerante", pedido2Apos.NomeProduto);
        Assert.AreEqual(5.00m, pedido2Apos.PrecoPraticado);

        // Calcular e verificar total após fechamento
        decimal totalAposFechar = pedido1Apos.Subtotal + pedido2Apos.Subtotal;
        Assert.AreEqual(totalAntesDeFechamento, totalAposFechar, "Total após fechamento deve ser igual ao total antes");
        Assert.AreEqual(22.00m, totalAposFechar, "Total deve ser 22.00");

        // Verificar também que a Conta em si foi persistida como Fechada
        dbContext.ChangeTracker.Clear();
        Conta? contaFinal = repositorioConta.SelecionarPorId(conta.Id);
        Assert.IsNotNull(contaFinal);
        Assert.AreEqual(StatusConta.Fechada, contaFinal.Status, "Conta deve permanecer Fechada após reload");
    }
}
