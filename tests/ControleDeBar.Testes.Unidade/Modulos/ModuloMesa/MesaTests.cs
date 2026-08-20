using ControleDeBar.Dominio.Modulos.ModuloMesa;

namespace ControleDeBar.Testes.Unidade.Modulos.ModuloMesa;

[TestClass]
public sealed class MesaTests
{
    [TestMethod]
    public void Deve_CriarMesa_ComDadosValidos()
    {
        // Arrange e Act
        Mesa mesa = new(5, 4);

        // Assert
        Assert.AreEqual(5, mesa.Numero);
        Assert.AreEqual(4, mesa.Lugares);
        Assert.AreEqual(StatusMesa.Livre, mesa.Status);
        Assert.IsEmpty(mesa.Validar());
    }

    [TestMethod]
    public void Deve_RejeitarMesa_ComNumeroInvalido()
    {
        // Arrange
        Mesa mesa = new(0, 4);

        // Act
        List<string> erros = mesa.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Número\" deve ser maior que zero.");
    }

    [TestMethod]
    public void Deve_RejeitarMesa_ComLugaresInvalidos()
    {
        // Arrange
        Mesa mesa = new(5, 0);

        // Act
        List<string> erros = mesa.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Lugares\" deve ser maior que zero.");
    }

    [TestMethod]
    public void Deve_RejeitarMesa_ComStatusIndeterminado()
    {
        // Arrange
        Mesa mesa = new(5, 4)
        {
            Status = StatusMesa.Indeterminado
        };

        // Act
        List<string> erros = mesa.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Status\" deve ser informado.");
    }

    [TestMethod]
    public void Deve_AtualizarMesa_ComNovosDados()
    {
        // Arrange
        Mesa mesa = new(5, 4);
        Mesa mesaAtualizada = new(6, 8);

        // Act
        mesa.Atualizar(mesaAtualizada);

        // Assert
        Assert.AreEqual(6, mesa.Numero);
        Assert.AreEqual(8, mesa.Lugares);
    }

    [TestMethod]
    public void Deve_PreservarStatus_AoAtualizarMesa()
    {
        // Arrange
        Mesa mesa = new(5, 4);
        mesa.Ocupar();

        Mesa mesaAtualizada = new(6, 8);

        // Act
        mesa.Atualizar(mesaAtualizada);

        // Assert
        Assert.AreEqual(StatusMesa.Ocupada, mesa.Status);
    }

    [TestMethod]
    public void Deve_AlterarStatus_QuandoOcuparELiberarMesa()
    {
        // Arrange
        Mesa mesa = new(5, 4);

        // Act e Assert
        mesa.Ocupar();
        Assert.AreEqual(StatusMesa.Ocupada, mesa.Status);

        mesa.Liberar();
        Assert.AreEqual(StatusMesa.Livre, mesa.Status);
    }
}
