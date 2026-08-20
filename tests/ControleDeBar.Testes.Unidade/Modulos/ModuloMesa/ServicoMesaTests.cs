using ControleDeBar.Aplicacao.Modulos.ModuloMesa;
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
}
