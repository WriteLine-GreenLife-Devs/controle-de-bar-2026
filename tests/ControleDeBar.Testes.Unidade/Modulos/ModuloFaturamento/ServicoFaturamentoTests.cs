using ControleDeBar.Aplicacao.Modulos.ModuloFaturamento;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using ControleDeBar.Dominio.Modulos.ModuloPedido;
using Moq;

namespace ControleDeBar.Testes.Unidade.Modulos.ModuloFaturamento;

[TestClass]
public sealed class ServicoFaturamentoTests
{
    private Mock<IRepositorioConta> repositorioContaMock = null!;
    private Mock<IRepositorioPedido> repositorioPedidoMock = null!;
    private Mock<IRepositorioMesa> repositorioMesaMock = null!;
    private ServicoFaturamento servicoFaturamento = null!;

    [TestInitialize]
    public void Inicializar()
    {
        repositorioContaMock = new Mock<IRepositorioConta>();
        repositorioPedidoMock = new Mock<IRepositorioPedido>();
        repositorioMesaMock = new Mock<IRepositorioMesa>();
        servicoFaturamento = new ServicoFaturamento(
            repositorioContaMock.Object,
            repositorioPedidoMock.Object,
            repositorioMesaMock.Object
        );
    }

    [TestMethod]
    public void Deve_RetornarZero_QuandoNaoHouverContasFechadasNaData()
    {
        ConfigurarDados([], []);

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(new DateTime(2026, 8, 24));

        Assert.AreEqual(0m, resultado.Total);
        Assert.IsEmpty(resultado.Contas);
        Assert.AreEqual(new DateTime(2026, 8, 24), resultado.Data);
    }

    [TestMethod]
    public void Deve_IgnorarContaAberta()
    {
        Conta conta = CriarConta("Aberta", StatusConta.Aberta, new DateTime(2026, 8, 24, 10, 0, 0));
        Pedido pedido = CriarPedido(conta.Id, 8m, 2);
        ConfigurarDados([conta], [pedido]);

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(conta.DataFechamento!.Value);

        Assert.IsEmpty(resultado.Contas);
        Assert.AreEqual(0m, resultado.Total);
    }

    [TestMethod]
    public void Deve_ConsiderarContaFechadaNaDataConsultada()
    {
        Conta conta = CriarConta("Carlos", StatusConta.Fechada, new DateTime(2026, 8, 24, 10, 0, 0));
        Pedido pedido = CriarPedido(conta.Id, 8m, 2);
        ConfigurarDados([conta], [pedido]);

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(new DateTime(2026, 8, 24, 23, 59, 0));

        Assert.HasCount(1, resultado.Contas);
        Assert.AreEqual(16m, resultado.Total);
        Assert.AreEqual("Carlos", resultado.Contas[0].NomeCliente);
    }

    [TestMethod]
    public void Deve_IgnorarContaFechadaEmOutraData()
    {
        Conta conta = CriarConta("Carlos", StatusConta.Fechada, new DateTime(2026, 8, 23, 23, 59, 0));
        ConfigurarDados([conta], [CriarPedido(conta.Id, 8m, 2)]);

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(new DateTime(2026, 8, 24));

        Assert.IsEmpty(resultado.Contas);
        Assert.AreEqual(0m, resultado.Total);
    }

    [TestMethod]
    public void Deve_UsarDataDeFechamentoEIgnorarDataDeAbertura()
    {
        Conta conta = CriarConta("Carlos", StatusConta.Fechada, new DateTime(2026, 8, 24, 10, 0, 0));
        conta.DataAbertura = new DateTime(2026, 8, 23, 23, 59, 0);
        ConfigurarDados([conta], [CriarPedido(conta.Id, 8m, 1)]);

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(new DateTime(2026, 8, 24));

        Assert.HasCount(1, resultado.Contas);
        Assert.AreEqual(new DateTime(2026, 8, 24, 10, 0, 0), resultado.Contas[0].DataFechamento);
    }

    [TestMethod]
    public void Deve_SomarPedidosDaConta()
    {
        Conta conta = CriarConta("Carlos", StatusConta.Fechada, new DateTime(2026, 8, 24, 10, 0, 0));
        ConfigurarDados(
            [conta],
            [CriarPedido(conta.Id, 8.50m, 2), CriarPedido(conta.Id, 5m, 1)]
        );

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(conta.DataFechamento!.Value);

        Assert.AreEqual(22m, resultado.Total);
        Assert.AreEqual(22m, resultado.Contas[0].Total);
    }

    [TestMethod]
    public void Deve_SomarTotaisDeVariasContas()
    {
        Conta primeiraConta = CriarConta("Carlos", StatusConta.Fechada, new DateTime(2026, 8, 24, 10, 0, 0));
        Conta segundaConta = CriarConta("Maria", StatusConta.Fechada, new DateTime(2026, 8, 24, 23, 59, 0));
        ConfigurarDados(
            [primeiraConta, segundaConta],
            [CriarPedido(primeiraConta.Id, 8.50m, 2), CriarPedido(segundaConta.Id, 5m, 1)]
        );

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(new DateTime(2026, 8, 24));

        Assert.HasCount(2, resultado.Contas);
        Assert.AreEqual(22m, resultado.Total);
    }

    [TestMethod]
    public void Deve_ConsiderarContaSemPedidosComTotalZero()
    {
        Conta conta = CriarConta("Carlos", StatusConta.Fechada, new DateTime(2026, 8, 24, 10, 0, 0));
        ConfigurarDados([conta], []);

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(conta.DataFechamento!.Value);

        Assert.HasCount(1, resultado.Contas);
        Assert.AreEqual(0m, resultado.Contas[0].Total);
        Assert.AreEqual(0m, resultado.Total);
    }

    [TestMethod]
    public void Deve_UsarPrecoPraticadoDoPedido()
    {
        Conta conta = CriarConta("Carlos", StatusConta.Fechada, new DateTime(2026, 8, 24, 10, 0, 0));
        ConfigurarDados([conta], [CriarPedido(conta.Id, 8m, 2)]);

        FaturamentoDiarioDto resultado = servicoFaturamento.Consultar(conta.DataFechamento!.Value);

        Assert.AreEqual(16m, resultado.Total);
    }

    private void ConfigurarDados(List<Conta> contas, List<Pedido> pedidos)
    {
        repositorioContaMock.Setup(repositorio => repositorio.SelecionarTodos()).Returns(contas);
        repositorioPedidoMock.Setup(repositorio => repositorio.SelecionarTodos()).Returns(pedidos);
        repositorioMesaMock.Setup(repositorio => repositorio.SelecionarTodos()).Returns(
            contas.Select(conta => new Mesa(1, 4) { Id = conta.MesaId }).ToList()
        );
    }

    private static Conta CriarConta(string nomeCliente, StatusConta status, DateTime dataFechamento)
    {
        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), nomeCliente)
        {
            Id = Guid.CreateVersion7(),
            Status = status,
            DataFechamento = dataFechamento
        };

        return conta;
    }

    private static Pedido CriarPedido(Guid contaId, decimal precoPraticado, int quantidade)
    {
        return new(contaId, Guid.CreateVersion7(), "Produto", precoPraticado, quantidade);
    }
}
