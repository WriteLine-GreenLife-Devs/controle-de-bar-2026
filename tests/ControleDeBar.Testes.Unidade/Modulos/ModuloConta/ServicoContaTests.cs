using ControleDeBar.Aplicacao.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using FluentResults;
using Moq;

namespace ControleDeBar.Testes.Unidade.Modulos.ModuloConta;

[TestClass]
public sealed class ServicoContaTests
{
    private Mock<IRepositorioConta> repositorioContaMock = null!;
    private Mock<IRepositorioMesa> repositorioMesaMock = null!;
    private Mock<IRepositorioGarcom> repositorioGarcomMock = null!;
    private ServicoConta servicoConta = null!;

    [TestInitialize]
    public void Inicializar()
    {
        repositorioContaMock = new Mock<IRepositorioConta>();
        repositorioMesaMock = new Mock<IRepositorioMesa>();
        repositorioGarcomMock = new Mock<IRepositorioGarcom>();
        servicoConta = new ServicoConta(
            repositorioContaMock.Object,
            repositorioMesaMock.Object,
            repositorioGarcomMock.Object
        );
    }

    [TestMethod]
    public void Deve_AbrirConta_ComDadosValidos()
    {
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();
        Mesa mesa = new(1, 4) { Id = mesaId };
        Garcom garcom = new("Marcos") { Id = garcomId };

        repositorioMesaMock.Setup(r => r.SelecionarPorId(mesaId)).Returns(mesa);
        repositorioGarcomMock.Setup(r => r.SelecionarPorId(garcomId)).Returns(garcom);

        Result resultado = servicoConta.Abrir(new AbrirContaDto(mesaId, garcomId, "Carlos"));

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(StatusMesa.Ocupada, mesa.Status);
        repositorioContaMock.Verify(r => r.Cadastrar(It.Is<Conta>(c =>
            c.MesaId == mesaId &&
            c.GarcomId == garcomId &&
            c.NomeCliente == "Carlos" &&
            c.Status == StatusConta.Aberta
        )), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarAbertura_ComMesaInexistente()
    {
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();

        repositorioMesaMock.Setup(r => r.SelecionarPorId(mesaId)).Returns((Mesa?)null);
        repositorioGarcomMock.Setup(r => r.SelecionarPorId(garcomId)).Returns(new Garcom("Marcos") { Id = garcomId });

        Result resultado = servicoConta.Abrir(new AbrirContaDto(mesaId, garcomId, "Carlos"));

        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Mesa não encontrada.", resultado.Errors[0].Message);
        repositorioContaMock.Verify(r => r.Cadastrar(It.IsAny<Conta>()), Times.Never);
    }

    [TestMethod]
    public void Deve_RejeitarAbertura_ComGarcomInexistente()
    {
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();
        Mesa mesa = new(1, 4) { Id = mesaId };

        repositorioMesaMock.Setup(r => r.SelecionarPorId(mesaId)).Returns(mesa);
        repositorioGarcomMock.Setup(r => r.SelecionarPorId(garcomId)).Returns((Garcom?)null);

        Result resultado = servicoConta.Abrir(new AbrirContaDto(mesaId, garcomId, "Carlos"));

        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Garçom não encontrado.", resultado.Errors[0].Message);
        repositorioContaMock.Verify(r => r.Cadastrar(It.IsAny<Conta>()), Times.Never);
    }

    [TestMethod]
    public void Deve_PermitirAbertura_DeDuasContasNaMesmaMesa()
    {
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();
        Mesa mesa = new(1, 4) { Id = mesaId };
        Garcom garcom = new("Marcos") { Id = garcomId };

        repositorioMesaMock.Setup(r => r.SelecionarPorId(mesaId)).Returns(mesa);
        repositorioGarcomMock.Setup(r => r.SelecionarPorId(garcomId)).Returns(garcom);

        Result primeira = servicoConta.Abrir(new AbrirContaDto(mesaId, garcomId, "Carlos"));
        Result segunda = servicoConta.Abrir(new AbrirContaDto(mesaId, garcomId, "Maria"));

        Assert.IsTrue(primeira.IsSuccess);
        Assert.IsTrue(segunda.IsSuccess);
        Assert.AreEqual(StatusMesa.Ocupada, mesa.Status);
        repositorioContaMock.Verify(r => r.Cadastrar(It.IsAny<Conta>()), Times.Exactly(2));
    }

    [TestMethod]
    public void Deve_FecharConta_Valida()
    {
        Guid id = Guid.CreateVersion7();
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();
        Conta conta = new(mesaId, garcomId, "Carlos") { Id = id };
        Mesa mesa = new(1, 4) { Id = mesaId };

        repositorioContaMock.Setup(r => r.SelecionarPorId(id)).Returns(conta);
        repositorioContaMock.Setup(r => r.SelecionarTodos()).Returns([conta]);
        repositorioMesaMock.Setup(r => r.SelecionarPorId(mesaId)).Returns(mesa);

        Result resultado = servicoConta.Fechar(id);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(StatusConta.Fechada, conta.Status);
        Assert.IsNotNull(conta.DataFechamento);
        Assert.AreEqual(StatusMesa.Livre, mesa.Status);
        repositorioContaMock.Verify(r => r.Salvar(), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarFechamento_DeContaInexistente()
    {
        Guid id = Guid.CreateVersion7();

        repositorioContaMock.Setup(r => r.SelecionarPorId(id)).Returns((Conta?)null);

        Result resultado = servicoConta.Fechar(id);

        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Conta não encontrada.", resultado.Errors[0].Message);
        repositorioContaMock.Verify(r => r.Salvar(), Times.Never);
    }

    [TestMethod]
    public void Deve_RejeitarFechamento_DeContaJaFechada()
    {
        Guid id = Guid.CreateVersion7();
        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Carlos") { Id = id };
        conta.Fechar();

        repositorioContaMock.Setup(r => r.SelecionarPorId(id)).Returns(conta);

        Result resultado = servicoConta.Fechar(id);

        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("A conta já está fechada.", resultado.Errors[0].Message);
    }

    [TestMethod]
    public void Deve_SelecionarSomenteContasAbertas()
    {
        Guid mesaAbertaId = Guid.CreateVersion7();
        Guid mesaFechadaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();

        Mesa mesaAberta = new(1, 4) { Id = mesaAbertaId };
        Mesa mesaFechada = new(2, 4) { Id = mesaFechadaId };
        Garcom garcom = new("Marcos") { Id = garcomId };

        Conta contaAberta1 = new(mesaAbertaId, garcomId, "Carlos");
        Conta contaAberta2 = new(mesaAbertaId, garcomId, "Maria");
        Conta contaFechada = new(mesaFechadaId, garcomId, "João");
        contaFechada.Fechar();

        repositorioMesaMock.Setup(r => r.SelecionarTodos()).Returns([mesaAberta, mesaFechada]);
        repositorioGarcomMock.Setup(r => r.SelecionarTodos()).Returns([garcom]);
        repositorioContaMock.Setup(r => r.SelecionarTodos()).Returns([contaAberta1, contaFechada, contaAberta2]);

        List<ListarContaDto> resultado = servicoConta.SelecionarAbertas();

        Assert.HasCount(2, resultado);
        Assert.IsTrue(resultado.All(c => c.Status == StatusConta.Aberta));
        Assert.IsTrue(resultado.Select(c => c.NomeCliente).Contains("Carlos"));
        Assert.IsTrue(resultado.Select(c => c.NomeCliente).Contains("Maria"));
    }

    [TestMethod]
    public void Deve_SelecionarSomenteContasFechadas()
    {
        Guid mesaAbertaId = Guid.CreateVersion7();
        Guid mesaFechadaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();

        Mesa mesaAberta = new(1, 4) { Id = mesaAbertaId };
        Mesa mesaFechada = new(2, 4) { Id = mesaFechadaId };
        Garcom garcom = new("Marcos") { Id = garcomId };

        Conta contaAberta = new(mesaAbertaId, garcomId, "Carlos");
        Conta contaFechada1 = new(mesaFechadaId, garcomId, "João");
        Conta contaFechada2 = new(mesaFechadaId, garcomId, "Maria");
        contaFechada1.Fechar();
        contaFechada2.Fechar();

        repositorioMesaMock.Setup(r => r.SelecionarTodos()).Returns([mesaAberta, mesaFechada]);
        repositorioGarcomMock.Setup(r => r.SelecionarTodos()).Returns([garcom]);
        repositorioContaMock.Setup(r => r.SelecionarTodos()).Returns([contaAberta, contaFechada1, contaFechada2]);

        List<ListarContaDto> resultado = servicoConta.SelecionarFechadas();

        Assert.HasCount(2, resultado);
        Assert.IsTrue(resultado.All(c => c.Status == StatusConta.Fechada));
        Assert.IsTrue(resultado.All(c => c.DataFechamento is not null));
        Assert.IsTrue(resultado.Select(c => c.NomeCliente).Contains("João"));
        Assert.IsTrue(resultado.Select(c => c.NomeCliente).Contains("Maria"));
    }

    [TestMethod]
    public void Deve_SelecionarTodos_QuandoHouverContasAbertasEFechadas()
    {
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();
        Mesa mesa = new(1, 4) { Id = mesaId };
        Garcom garcom = new("Marcos") { Id = garcomId };

        Conta contaAberta = new(mesaId, garcomId, "Carlos");
        Conta contaFechada = new(mesaId, garcomId, "Maria");
        contaFechada.Fechar();

        repositorioMesaMock.Setup(r => r.SelecionarTodos()).Returns([mesa]);
        repositorioGarcomMock.Setup(r => r.SelecionarTodos()).Returns([garcom]);
        repositorioContaMock.Setup(r => r.SelecionarTodos()).Returns([contaAberta, contaFechada]);

        List<ListarContaDto> resultado = servicoConta.SelecionarTodos();

        Assert.HasCount(2, resultado);
        Assert.IsTrue(resultado.Any(c => c.NomeCliente == "Carlos" && c.Status == StatusConta.Aberta));
        Assert.IsTrue(resultado.Any(c => c.NomeCliente == "Maria" && c.Status == StatusConta.Fechada));
    }

    [TestMethod]
    public void Deve_ManterMesaOcupada_QuandoAindaExisteOutraContaAberta()
    {
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();
        Guid contaFechadaId = Guid.CreateVersion7();
        Guid contaAbertaId = Guid.CreateVersion7();
        Mesa mesa = new(1, 4) { Id = mesaId, Status = StatusMesa.Ocupada };
        Conta contaFechada = new(mesaId, garcomId, "Carlos") { Id = contaFechadaId };
        Conta contaAberta = new(mesaId, garcomId, "Maria") { Id = contaAbertaId };

        repositorioContaMock.Setup(r => r.SelecionarPorId(contaFechadaId)).Returns(contaFechada);
        repositorioContaMock.Setup(r => r.SelecionarTodos()).Returns([contaFechada, contaAberta]);
        repositorioMesaMock.Setup(r => r.SelecionarPorId(mesaId)).Returns(mesa);

        Result resultado = servicoConta.Fechar(contaFechadaId);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(StatusConta.Fechada, contaFechada.Status);
        Assert.AreEqual(StatusMesa.Ocupada, mesa.Status);
    }
}
