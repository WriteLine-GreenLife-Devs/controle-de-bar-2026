using ControleDeBar.Aplicacao.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloProduto;
using FluentResults;
using Moq;

namespace ControleDeBar.Testes.Unidade.Modulos.ModuloGarcom;

[TestClass]
public sealed class ServicoGarcomTests
{
    private Mock<IRepositorioGarcom> repositorioGarcomMock = null!;
    private Mock<IRepositorioConta> repositorioContaMock = null!;
    private ServicoGarcom servicoGarcom = null!;

    [TestInitialize]
    public void Inicializar()
    {
        repositorioGarcomMock = new Mock<IRepositorioGarcom>();
        repositorioContaMock = new Mock<IRepositorioConta>();
        servicoGarcom = new ServicoGarcom(
            repositorioGarcomMock.Object,
            repositorioContaMock.Object
        );
    }

    [TestMethod]
    public void Deve_CadastrarGarcom_ComDadosValidos()
    {
        // Arrange
        CadastrarGarcomDto dto = new("  Marcos  ");

        repositorioGarcomMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        Result resultado = servicoGarcom.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioGarcomMock.Verify(
            r => r.Cadastrar(It.Is<Garcom>(g => g.Nome == "Marcos")),
            Times.Once
        );
    }

    [TestMethod]
    [DataRow("Marcos")]
    [DataRow("MARCOS")]
    public void Deve_RejeitarCadastro_ComNomeDuplicado(string nome)
    {
        // Arrange
        CadastrarGarcomDto dto = new(nome);

        repositorioGarcomMock
            .Setup(r => r.SelecionarTodos())
            .Returns([new Garcom("Marcos")]);

        // Act
        Result resultado = servicoGarcom.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Já existe um garçom com este nome.", resultado.Errors[0].Message);
        repositorioGarcomMock.Verify(r => r.Cadastrar(It.IsAny<Garcom>()), Times.Never);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Deve_RejeitarCadastro_ComNomeInvalido(string nome)
    {
        // Arrange
        CadastrarGarcomDto dto = new(nome);

        repositorioGarcomMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        Result resultado = servicoGarcom.Cadastrar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("O campo \"Nome\" é obrigatório.", resultado.Errors[0].Message);
        repositorioGarcomMock.Verify(r => r.Cadastrar(It.IsAny<Garcom>()), Times.Never);
    }

    [TestMethod]
    public void Deve_EditarGarcom_ComDadosValidos()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        EditarGarcomDto dto = new(id, "  Paulo  ");

        repositorioGarcomMock.Setup(r => r.SelecionarTodos()).Returns([]);
        repositorioGarcomMock.Setup(r => r.Editar(id, It.IsAny<Garcom>())).Returns(true);

        // Act
        Result resultado = servicoGarcom.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioGarcomMock.Verify(
            r => r.Editar(id, It.Is<Garcom>(g => g.Nome == "Paulo")),
            Times.Once
        );
    }

    [TestMethod]
    public void Deve_RejeitarEdicao_DeGarcomInexistente()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        EditarGarcomDto dto = new(id, "Paulo");

        repositorioGarcomMock.Setup(r => r.SelecionarTodos()).Returns([]);
        repositorioGarcomMock.Setup(r => r.Editar(id, It.IsAny<Garcom>())).Returns(false);

        // Act
        Result resultado = servicoGarcom.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Garçom não encontrado.", resultado.Errors[0].Message);
    }

    [TestMethod]
    public void Deve_RejeitarEdicao_ComNomeDuplicado()
    {
        // Arrange
        Guid idGarcomEditado = Guid.CreateVersion7();
        Garcom garcomExistente = new("Marcos")
        {
            Id = Guid.CreateVersion7()
        };

        EditarGarcomDto dto = new(idGarcomEditado, "MARCOS");

        repositorioGarcomMock
            .Setup(r => r.SelecionarTodos())
            .Returns([garcomExistente]);

        // Act
        Result resultado = servicoGarcom.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Já existe um garçom com este nome.", resultado.Errors[0].Message);
        repositorioGarcomMock.Verify(
            r => r.Editar(It.IsAny<Guid>(), It.IsAny<Garcom>()),
            Times.Never
        );
    }

    [TestMethod]
    public void Deve_PermitirEdicao_MantendoNomeDoProprioGarcom()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        Garcom garcom = new("Marcos") { Id = id };
        EditarGarcomDto dto = new(id, "Marcos");

        repositorioGarcomMock.Setup(r => r.SelecionarTodos()).Returns([garcom]);
        repositorioGarcomMock.Setup(r => r.Editar(id, It.IsAny<Garcom>())).Returns(true);

        // Act
        Result resultado = servicoGarcom.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioGarcomMock.Verify(r => r.Editar(id, It.IsAny<Garcom>()), Times.Once);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Deve_RejeitarEdicao_ComNomeInvalido(string nome)
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        EditarGarcomDto dto = new(id, nome);

        repositorioGarcomMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        Result resultado = servicoGarcom.Editar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("O campo \"Nome\" é obrigatório.", resultado.Errors[0].Message);
        repositorioGarcomMock.Verify(
            r => r.Editar(It.IsAny<Guid>(), It.IsAny<Garcom>()),
            Times.Never
        );
    }

    [TestMethod]
    public void Deve_ExcluirGarcom_Existente()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        Garcom garcom = new("Marcos") { Id = id };

        repositorioGarcomMock.Setup(r => r.SelecionarPorId(id)).Returns(garcom);
        repositorioContaMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        Result resultado = servicoGarcom.Excluir(id);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        repositorioGarcomMock.Verify(r => r.Excluir(id), Times.Once);
    }

    [TestMethod]
    public void Deve_RejeitarExclusao_DeGarcomInexistente()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();

        repositorioGarcomMock.Setup(r => r.SelecionarPorId(id)).Returns((Garcom?)null);

        // Act
        Result resultado = servicoGarcom.Excluir(id);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Garçom não encontrado.", resultado.Errors[0].Message);
        repositorioGarcomMock.Verify(r => r.Excluir(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public void Deve_RejeitarExclusao_DeGarcomComContaAberta()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        Garcom garcom = new("Marcos") { Id = id };
        Conta conta = new(Guid.CreateVersion7(), id, "Carlos")
        {
            Status = StatusConta.Aberta
        };

        repositorioGarcomMock.Setup(r => r.SelecionarPorId(id)).Returns(garcom);
        repositorioContaMock.Setup(r => r.SelecionarTodos()).Returns([conta]);

        // Act
        Result resultado = servicoGarcom.Excluir(id);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(
            "Não é possível excluir este garçom, pois ele está vinculado a uma conta.",
            resultado.Errors[0].Message
        );
        repositorioGarcomMock.Verify(r => r.Excluir(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public void Deve_RejeitarExclusao_DeGarcomComContaFechada()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        Garcom garcom = new("Marcos") { Id = id };
        Conta conta = new(Guid.CreateVersion7(), id, "Carlos");
        conta.Fechar();

        repositorioGarcomMock.Setup(r => r.SelecionarPorId(id)).Returns(garcom);
        repositorioContaMock.Setup(r => r.SelecionarTodos()).Returns([conta]);

        // Act
        Result resultado = servicoGarcom.Excluir(id);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(
            "Não é possível excluir este garçom, pois ele está vinculado a uma conta.",
            resultado.Errors[0].Message
        );
        repositorioGarcomMock.Verify(r => r.Excluir(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public void Deve_SelecionarTodosOsGarcons()
    {
        // Arrange
        Garcom primeiroGarcom = new("Marcos");
        Garcom segundoGarcom = new("Paulo");

        repositorioGarcomMock
            .Setup(r => r.SelecionarTodos())
            .Returns([primeiroGarcom, segundoGarcom]);

        // Act
        List<ListarGarcomDto> garcons = servicoGarcom.SelecionarTodos();

        // Assert
        Assert.HasCount(2, garcons);
        Assert.AreEqual("Marcos", garcons[0].Nome);
        Assert.AreEqual("Paulo", garcons[1].Nome);
    }

    [TestMethod]
    public void Deve_RetornarListaVazia_QuandoNaoHouverGarcons()
    {
        // Arrange
        repositorioGarcomMock.Setup(r => r.SelecionarTodos()).Returns([]);

        // Act
        List<ListarGarcomDto> garcons = servicoGarcom.SelecionarTodos();

        // Assert
        Assert.IsEmpty(garcons);
    }

    [TestMethod]
    public void Deve_SelecionarGarcom_PorId()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();
        Garcom garcom = new("Marcos") { Id = id };

        repositorioGarcomMock.Setup(r => r.SelecionarPorId(id)).Returns(garcom);

        // Act
        Result<DetalhesGarcomDto> resultado = servicoGarcom.SelecionarPorId(id);

        // Assert
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(id, resultado.Value.Id);
        Assert.AreEqual("Marcos", resultado.Value.Nome);
    }

    [TestMethod]
    public void Deve_RejeitarSelecao_DeGarcomInexistente()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();

        repositorioGarcomMock.Setup(r => r.SelecionarPorId(id)).Returns((Garcom?)null);

        // Act
        Result<DetalhesGarcomDto> resultado = servicoGarcom.SelecionarPorId(id);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Garçom não encontrado.", resultado.Errors[0].Message);
    }
}
