using ControleDeBar.Dominio.Modulos.ModuloConta;

namespace ControleDeBar.Testes.Unidade.Modulos.ModuloConta;

[TestClass]
public sealed class ContaTests
{
    [TestMethod]
    public void Deve_CriarConta_Valida()
    {
        Guid mesaId = Guid.CreateVersion7();
        Guid garcomId = Guid.CreateVersion7();

        Conta conta = new(mesaId, garcomId, "  Carlos  ");

        Assert.AreEqual(mesaId, conta.MesaId);
        Assert.AreEqual(garcomId, conta.GarcomId);
        Assert.AreEqual("Carlos", conta.NomeCliente);
        Assert.AreEqual(StatusConta.Aberta, conta.Status);
        Assert.IsNotNull(conta.DataAbertura);
        Assert.IsNull(conta.DataFechamento);
        Assert.IsEmpty(conta.Validar());
    }

    [TestMethod]
    public void Deve_RejeitarConta_ComNomeClienteVazio()
    {
        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "   ");

        List<string> erros = conta.Validar();

        CollectionAssert.Contains(erros, "O campo \"Nome do cliente\" é obrigatório.");
    }

    [TestMethod]
    public void Deve_RejeitarConta_ComMesaVazia()
    {
        Conta conta = new(Guid.Empty, Guid.CreateVersion7(), "Carlos");

        List<string> erros = conta.Validar();

        CollectionAssert.Contains(erros, "O campo \"Mesa\" é obrigatório.");
    }

    [TestMethod]
    public void Deve_RejeitarConta_ComGarcomVazio()
    {
        Conta conta = new(Guid.CreateVersion7(), Guid.Empty, "Carlos");

        List<string> erros = conta.Validar();

        CollectionAssert.Contains(erros, "O campo \"Garçom\" é obrigatório.");
    }

    [TestMethod]
    public void Deve_FecharConta_AlterandoStatusERegistrandoDataFechamento()
    {
        Conta conta = new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Carlos");

        conta.Fechar();

        Assert.AreEqual(StatusConta.Fechada, conta.Status);
        Assert.IsNotNull(conta.DataFechamento);
    }
}
