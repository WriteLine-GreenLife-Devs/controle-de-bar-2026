using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloProduto;

namespace ControleDeBar.Testes.Unidade.Modulos.ModuloGarcom;

[TestClass]
public sealed class GarcomTests
{
    [TestMethod]
    public void Deve_CriarGarcom_ComDadosValidos()
    {
        // Arrange e Act
        Garcom garcom = new("Marcos");

        // Assert
        Assert.AreEqual("Marcos", garcom.Nome);
        Assert.IsEmpty(garcom.Validar());
    }

    [TestMethod]
    public void Deve_RemoverEspacosExternos_AoCriarGarcom()
    {
        // Arrange e Act
        Garcom garcom = new("  Marcos  ");

        // Assert
        Assert.AreEqual("Marcos", garcom.Nome);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Deve_RejeitarGarcom_ComNomeInvalido(string nome)
    {
        // Arrange
        Garcom garcom = new(nome);

        // Act
        List<string> erros = garcom.Validar();

        // Assert
        CollectionAssert.Contains(erros, "O campo \"Nome\" é obrigatório.");
    }

    [TestMethod]
    public void Deve_AtualizarGarcom_ComNovoNome()
    {
        // Arrange
        Garcom garcom = new("Marcos");
        Garcom garcomAtualizado = new("Paulo");

        // Act
        garcom.Atualizar(garcomAtualizado);

        // Assert
        Assert.AreEqual("Paulo", garcom.Nome);
    }

    [TestMethod]
    public void Deve_PreservarUserId_AoAtualizarGarcom()
    {
        // Arrange
        Guid userId = Guid.CreateVersion7();
        Garcom garcom = new("Marcos") { UserId = userId };
        Garcom garcomAtualizado = new("Paulo") { UserId = Guid.CreateVersion7() };

        // Act
        garcom.Atualizar(garcomAtualizado);

        // Assert
        Assert.AreEqual(userId, garcom.UserId);
    }
}
