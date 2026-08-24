using ControleDeBar.Aplicacao.Modulos.ModuloMesa;
using ControleDeBar.Aplicacao.Modulos.ModuloProduto;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using FluentResults;
using Moq;

namespace ControleDeBar.Testes.Unidade.Modulos.ModuloMesa;

[TestClass]
public sealed class ServicoMesaTests
{
    private Mock<IRepositorioMesa> repositorioMesaMock = null!;
    private ServicoMesa servicoMesa = null!;

    [TestInitialize]
    public void Inicializar()
    {
        repositorioMesaMock = new Mock<IRepositorioMesa>();
        servicoMesa = new ServicoMesa(repositorioMesaMock.Object);
    }

    [TestMethod]
    public void Deve_CadastrarMesa_ComDadosValidos()
    {
        // Arrange
        CadastrarMesaDto dto = new(1, 4);

        repositorioMesaMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        Result resultado = servicoMesa.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioMesaMock.Verify(r => r.Cadastrar(It.Is<Mesa>(m =>
            m.Numero == 1 &&
            m.Lugares == 4 &&
            m.Status == StatusMesa.Livre
        )), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarCadastro_ComNumeroDuplicado()
    {
        // Arrange
        CadastrarMesaDto dto = new(1, 4);

        repositorioMesaMock.Setup(r => r.SelecionarTodos()).Returns([new Mesa(1, 2)]);

        // Act
        Result resultado = servicoMesa.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Já existe uma mesa com este número.", resultado.Errors[0].Message);
        repositorioMesaMock.Verify(r => r.Cadastrar(It.IsAny<Mesa>()), Times.Never);
    }

    [TestMethod]
    [DataRow(0, 4, "O campo \"Número\" deve ser maior que zero.")]
    [DataRow(1, 0, "O campo \"Lugares\" deve ser maior que zero.")]
    public void Deve_RejeitarCadastro_ComDadosInvalidos(
        int numero,
        int lugares,
        string mensagemEsperada
    )
    {
        // Arrange
        CadastrarMesaDto dto = new(numero, lugares);

        repositorioMesaMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        Result resultado = servicoMesa.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(mensagemEsperada, resultado.Errors[0].Message);
        repositorioMesaMock.Verify(r => r.Cadastrar(It.IsAny<Mesa>()), Times.Never);
    }

    [TestMethod]
    public void Deve_EditarMesa_ComDadosValidos()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        EditarMesaDto dto = new(id, 2, 6);

        repositorioMesaMock.Setup(r => r.SelecionarTodos()).Returns([]);
        repositorioMesaMock.Setup(r => r.Editar(id, It.IsAny<Mesa>())).Returns(true);

        // Act
        Result resultado = servicoMesa.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioMesaMock.Verify(r => r.Editar(id, It.Is<Mesa>(m =>
            m.Numero == 2 && m.Lugares == 6
        )), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarEdicao_DeMesaInexistente()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        EditarMesaDto dto = new(id, 2, 6);

        repositorioMesaMock.Setup(r => r.SelecionarTodos()).Returns([]);
        repositorioMesaMock.Setup(r => r.Editar(id, It.IsAny<Mesa>())).Returns(false);

        // Act
        Result resultado = servicoMesa.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Mesa não encontrada.", resultado.Errors[0].Message);
    }

    [TestMethod]
    public void Deve_RejeitarEdicao_ComNumeroDuplicado()
    {
        // Arrange
        Guid idMesaEditada = Guid.CreateVersion7();
        Mesa mesaExistente = new(1, 4)
        {
            Id = Guid.CreateVersion7()
        };

        EditarMesaDto dto = new(idMesaEditada, 1, 6);

        repositorioMesaMock.Setup(r => r.SelecionarTodos()).Returns([mesaExistente]);

        // Act
        Result resultado = servicoMesa.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Já existe uma mesa com este número.", resultado.Errors[0].Message);
        repositorioMesaMock.Verify(
            r => r.Editar(It.IsAny<Guid>(), It.IsAny<Mesa>()),
            Times.Never
        );
    }

    [TestMethod]
    [DataRow(0, 4, "O campo \"Número\" deve ser maior que zero.")]
    [DataRow(1, 0, "O campo \"Lugares\" deve ser maior que zero.")]
    public void Deve_RejeitarEdicao_ComDadosInvalidos(
        int numero,
        int lugares,
        string mensagemEsperada
    )
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        EditarMesaDto dto = new(id, numero, lugares);

        repositorioMesaMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        Result resultado = servicoMesa.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(mensagemEsperada, resultado.Errors[0].Message);
        repositorioMesaMock.Verify(
            r => r.Editar(It.IsAny<Guid>(), It.IsAny<Mesa>()),
            Times.Never
        );
    }

    [TestMethod]
    public void Deve_ExcluirMesa_Existente()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        Mesa mesa = new(1, 4) { Id = id };

        repositorioMesaMock.Setup(r => r.SelecionarPorId(id)).Returns(mesa);

        // Act
        Result resultado = servicoMesa.Excluir(id);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioMesaMock.Verify(r => r.Excluir(id), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarExclusao_DeMesaInexistente()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();

        repositorioMesaMock.Setup(r => r.SelecionarPorId(id)).Returns((Mesa?)null);

        // Act
        Result resultado = servicoMesa.Excluir(id);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Mesa não encontrada.", resultado.Errors[0].Message);
        repositorioMesaMock.Verify(r => r.Excluir(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public void Deve_SelecionarTodasAsMesas()
    {
        // Arrange
        Mesa primeiraMesa = new(1, 4);
        Mesa segundaMesa = new(2, 6);
        segundaMesa.Ocupar();

        repositorioMesaMock
            .Setup(r => r.SelecionarTodos())
            .Returns([primeiraMesa, segundaMesa]);

        // Act
        List<ListarMesaDto> mesas = servicoMesa.SelecionarTodos();

        // Assert
        Assert.HasCount(2, mesas);
        Assert.AreEqual(1, mesas[0].Numero);
        Assert.AreEqual(StatusMesa.Livre, mesas[0].Status);
        Assert.AreEqual(2, mesas[1].Numero);
        Assert.AreEqual(StatusMesa.Ocupada, mesas[1].Status);
    }

    [TestMethod]
    public void Deve_RetornarListaVazia_QuandoNaoHouverMesas()
    {
        // Arrange
        repositorioMesaMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        List<ListarMesaDto> mesas = servicoMesa.SelecionarTodos();

        // Assert
        Assert.IsEmpty(mesas);
    }

    [TestMethod]
    public void Deve_SelecionarMesa_PorId()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        Mesa mesa = new(1, 4) { Id = id };

        repositorioMesaMock.Setup(r => r.SelecionarPorId(id)).Returns(mesa);

        // Act
        Result<DetalhesMesaDto> resultado = servicoMesa.SelecionarPorId(id);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(id, resultado.Value.Id);
        Assert.AreEqual(1, resultado.Value.Numero);
        Assert.AreEqual(4, resultado.Value.Lugares);
        Assert.AreEqual(StatusMesa.Livre, resultado.Value.Status);
    }

    [TestMethod]
    public void Deve_RejeitarSelecao_DeMesaInexistente()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();

        repositorioMesaMock.Setup(r => r.SelecionarPorId(id)).Returns((Mesa?)null);

        // Act
        Result<DetalhesMesaDto> resultado = servicoMesa.SelecionarPorId(id);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Mesa não encontrada.", resultado.Errors[0].Message);
    }
}
