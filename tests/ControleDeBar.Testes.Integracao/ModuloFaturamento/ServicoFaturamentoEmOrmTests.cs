using ControleDeBar.Aplicacao.Modulos.ModuloFaturamento;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using ControleDeBar.Dominio.Modulos.ModuloPedido;
using ControleDeBar.Dominio.Modulos.ModuloProduto;
using ControleDeBar.Infra.Modulos.ModuloConta;
using ControleDeBar.Infra.Modulos.ModuloGarcom;
using ControleDeBar.Infra.Modulos.ModuloMesa;
using ControleDeBar.Infra.Modulos.ModuloPedido;
using ControleDeBar.Infra.Modulos.ModuloProduto;
using ControleDeBar.Testes.Integracao.Compartilhado.Orm;

namespace ControleDeBar.Testes.Integracao.ModuloFaturamento;

[TestClass]
public sealed class ServicoFaturamentoEmOrmTests : RepositorioBaseEmOrmTests
{
    private RepositorioContaEmOrm repositorioConta = null!;
    private RepositorioPedidoEmOrm repositorioPedido = null!;
    private RepositorioProdutoEmOrm repositorioProduto = null!;
    private ServicoFaturamento servicoFaturamento = null!;

    [TestInitialize]
    public override void InicializarContexto()
    {
        base.InicializarContexto();
        repositorioConta = new RepositorioContaEmOrm(dbContext);
        repositorioPedido = new RepositorioPedidoEmOrm(dbContext);
        repositorioProduto = new RepositorioProdutoEmOrm(dbContext);
        servicoFaturamento = new ServicoFaturamento(
            repositorioConta,
            repositorioPedido,
            repositorioMesa
        );
    }

    [TestMethod]
    public void Deve_SomarContasFechadasDoDia_EIgnorarOutrasDatasEContasAbertas()
    {
        DateTime dataConsulta = new(2026, 8, 24);
        Produto produto = CriarProduto("Cerveja", 8.50m);
        Conta primeiraConta = CriarConta("Carlos", 1, StatusConta.Fechada, new DateTime(2026, 8, 24, 10, 0, 0));
        Conta segundaConta = CriarConta("Maria", 2, StatusConta.Fechada, new DateTime(2026, 8, 24, 23, 59, 0));
        Conta outraData = CriarConta("João", 3, StatusConta.Fechada, new DateTime(2026, 8, 23, 23, 59, 0));
        Conta contaAberta = CriarConta("Ana", 4, StatusConta.Aberta, null);

        repositorioPedido.Cadastrar(new Pedido(primeiraConta.Id, produto.Id, produto.Nome, produto.Preco, 2));
        repositorioPedido.Cadastrar(new Pedido(segundaConta.Id, produto.Id, produto.Nome, produto.Preco, 1));
        repositorioPedido.Cadastrar(new Pedido(outraData.Id, produto.Id, produto.Nome, produto.Preco, 10));
        repositorioPedido.Cadastrar(new Pedido(contaAberta.Id, produto.Id, produto.Nome, produto.Preco, 20));
        dbContext.ChangeTracker.Clear();

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(dataConsulta);

        Assert.HasCount(2, resultado.Contas);
        Assert.AreEqual(25.50m, resultado.Total);
        Assert.IsTrue(resultado.Contas.All(conta =>
            conta.NomeCliente is "Carlos" or "Maria"));
        Assert.IsFalse(resultado.Contas.Any(conta =>
            conta.NomeCliente is "João" or "Ana"));
    }

    [TestMethod]
    public void Deve_IsolarContasEPedidosDeOutroUsuario()
    {
        DateTime dataConsulta = new(2026, 8, 24);
        Produto produto = CriarProduto("Cerveja", 8.50m);
        Conta contaUsuarioAtual = CriarConta("Carlos", 1, StatusConta.Fechada, new DateTime(2026, 8, 24, 10, 0, 0));
        Pedido pedidoUsuarioAtual = new(contaUsuarioAtual.Id, produto.Id, produto.Nome, produto.Preco, 2);
        repositorioPedido.Cadastrar(pedidoUsuarioAtual);

        Guid outroUsuarioId = Guid.CreateVersion7();
        using ControleDeBar.Infra.Compartilhado.Orm.ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId);
        RepositorioMesaEmOrm outroRepositorioMesa = new(outroContexto);
        RepositorioGarcomEmOrm outroRepositorioGarcom = new(outroContexto);
        RepositorioProdutoEmOrm outroRepositorioProduto = new(outroContexto);
        RepositorioContaEmOrm outroRepositorioConta = new(outroContexto);
        RepositorioPedidoEmOrm outroRepositorioPedido = new(outroContexto);

        Mesa mesaOutroUsuario = new(10, 4);
        Garcom garcomOutroUsuario = new("Rafael");
        Produto produtoOutroUsuario = new("Chopp", 100m);
        outroRepositorioMesa.Cadastrar(mesaOutroUsuario);
        outroRepositorioGarcom.Cadastrar(garcomOutroUsuario);
        outroRepositorioProduto.Cadastrar(produtoOutroUsuario);

        Conta contaOutroUsuario = new(mesaOutroUsuario.Id, garcomOutroUsuario.Id, "Outro usuário");
        outroRepositorioConta.Cadastrar(contaOutroUsuario);
        contaOutroUsuario.Status = StatusConta.Fechada;
        contaOutroUsuario.DataFechamento = new DateTime(2026, 8, 24, 23, 59, 0);
        outroContexto.SaveChanges();
        outroRepositorioPedido.Cadastrar(new Pedido(
            contaOutroUsuario.Id,
            produtoOutroUsuario.Id,
            produtoOutroUsuario.Nome,
            produtoOutroUsuario.Preco,
            10
        ));

        dbContext.ChangeTracker.Clear();

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(dataConsulta);

        Assert.HasCount(1, resultado.Contas);
        Assert.AreEqual("Carlos", resultado.Contas[0].NomeCliente);
        Assert.AreEqual(17m, resultado.Total);
    }

    [TestMethod]
    public void Deve_RetornarZero_QuandoNaoHouverContasFechadasNaData()
    {
        Conta conta = CriarConta("Carlos", 1, StatusConta.Fechada, new DateTime(2026, 8, 23, 23, 59, 0));

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(new DateTime(2026, 8, 24));

        Assert.AreEqual(new DateTime(2026, 8, 24), resultado.Data);
        Assert.AreEqual(0m, resultado.Total);
        Assert.IsEmpty(resultado.Contas);
        Assert.IsNotNull(conta);
    }

    private Produto CriarProduto(string nome, decimal preco)
    {
        Produto produto = new(nome, preco);
        repositorioProduto.Cadastrar(produto);
        return produto;
    }

    private Conta CriarConta(string nomeCliente, int numeroMesa, StatusConta status, DateTime? dataFechamento)
    {
        Mesa mesa = new(numeroMesa, 4);
        Garcom garcom = new($"Garçom {numeroMesa}");
        repositorioMesa.Cadastrar(mesa);
        repositorioGarcom.Cadastrar(garcom);

        Conta conta = new(mesa.Id, garcom.Id, nomeCliente);
        repositorioConta.Cadastrar(conta);
        conta.Status = status;
        conta.DataFechamento = dataFechamento;
        dbContext.SaveChanges();
        return conta;
    }
}
