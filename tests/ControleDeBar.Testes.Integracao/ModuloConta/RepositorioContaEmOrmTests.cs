using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using ControleDeBar.Infra.Compartilhado.Orm;
using ControleDeBar.Infra.Modulos.ModuloConta;
using ControleDeBar.Infra.Modulos.ModuloGarcom;
using ControleDeBar.Infra.Modulos.ModuloMesa;
using ControleDeBar.Testes.Integracao.Compartilhado.Orm;

namespace ControleDeBar.Testes.Integracao.ModuloConta;

[TestClass]
public sealed class RepositorioContaEmOrmTests : RepositorioBaseEmOrmTests
{
    private RepositorioContaEmOrm repositorioConta = null!;

    [TestInitialize]
    public void InicializarConta()
    {
        repositorioConta = new RepositorioContaEmOrm(dbContext);
    }

    [TestMethod]
    public void CadastrarESelecionarPorId_CarregaRegistro()
    {
        Mesa mesa = new(1, 4);
        Garcom garcom = new("Marcos");

        repositorioMesa.Cadastrar(mesa);
        repositorioGarcom.Cadastrar(garcom);

        Conta conta = new(mesa.Id, garcom.Id, "Carlos");

        repositorioConta.Cadastrar(conta);
        dbContext.ChangeTracker.Clear();

        Conta? contaSelecionada = repositorioConta.SelecionarPorId(conta.Id);

        Assert.IsNotNull(contaSelecionada);
        Assert.AreEqual("Carlos", contaSelecionada.NomeCliente);
        Assert.AreEqual(mesa.Id, contaSelecionada.MesaId);
        Assert.AreEqual(garcom.Id, contaSelecionada.GarcomId);
    }

    [TestMethod]
    public void Cadastrar_PreencheUserIdDoUsuarioAutenticado()
    {
        Mesa mesa = new(1, 4);
        Garcom garcom = new("Marcos");
        repositorioMesa.Cadastrar(mesa);
        repositorioGarcom.Cadastrar(garcom);

        Conta conta = new(mesa.Id, garcom.Id, "Carlos");

        repositorioConta.Cadastrar(conta);
        dbContext.ChangeTracker.Clear();

        Conta? contaSelecionada = repositorioConta.SelecionarPorId(conta.Id);

        Assert.IsNotNull(contaSelecionada);
        Assert.AreEqual(userId, contaSelecionada.UserId);
    }

    [TestMethod]
    public void SelecionarPorId_NaoCarregaContaDeOutroUsuario()
    {
        Guid outroUsuarioId = Guid.CreateVersion7();
        using ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId);
        RepositorioContaEmOrm outroRepositorio = new(outroContexto);
        RepositorioMesaEmOrm outroRepositorioMesa = new(outroContexto);
        RepositorioGarcomEmOrm outroRepositorioGarcom = new(outroContexto);

        Mesa mesa = new(1, 4);
        Garcom garcom = new("Marcos");
        outroRepositorioMesa.Cadastrar(mesa);
        outroRepositorioGarcom.Cadastrar(garcom);

        Conta contaOutroUsuario = new(mesa.Id, garcom.Id, "Carlos");
        outroRepositorio.Cadastrar(contaOutroUsuario);

        dbContext.ChangeTracker.Clear();

        Conta? contaSelecionada = repositorioConta.SelecionarPorId(contaOutroUsuario.Id);

        Assert.IsNull(contaSelecionada);
    }

    [TestMethod]
    public void SelecionarTodos_CarregaSomenteContasDoUsuarioAutenticado()
    {
        Mesa mesa = new(1, 4);
        Garcom garcom = new("Marcos");
        repositorioMesa.Cadastrar(mesa);
        repositorioGarcom.Cadastrar(garcom);

        repositorioConta.Cadastrar(new Conta(mesa.Id, garcom.Id, "Carlos"));
        repositorioConta.Cadastrar(new Conta(mesa.Id, garcom.Id, "Maria"));

        Guid outroUsuarioId = Guid.CreateVersion7();
        using ControleDeBarDbContext outroContexto = CriarDbContext(outroUsuarioId);
        RepositorioContaEmOrm outroRepositorio = new(outroContexto);
        RepositorioMesaEmOrm outroRepositorioMesa = new(outroContexto);
        RepositorioGarcomEmOrm outroRepositorioGarcom = new(outroContexto);

        Mesa mesaOutroUsuario = new(2, 4);
        Garcom garcomOutroUsuario = new("Rafael");
        outroRepositorioMesa.Cadastrar(mesaOutroUsuario);
        outroRepositorioGarcom.Cadastrar(garcomOutroUsuario);
        outroRepositorio.Cadastrar(new Conta(mesaOutroUsuario.Id, garcomOutroUsuario.Id, "João"));

        dbContext.ChangeTracker.Clear();

        List<Conta> contas = repositorioConta.SelecionarTodos();

        Assert.HasCount(2, contas);
        Assert.IsTrue(contas.All(c => c.UserId == userId));
    }

    [TestMethod]
    public void PermiteMultiplasContasNaMesmaMesa()
    {
        Mesa mesa = new(1, 4);
        Garcom garcom = new("Marcos");
        repositorioMesa.Cadastrar(mesa);
        repositorioGarcom.Cadastrar(garcom);

        repositorioConta.Cadastrar(new Conta(mesa.Id, garcom.Id, "Carlos"));
        repositorioConta.Cadastrar(new Conta(mesa.Id, garcom.Id, "Maria"));

        List<Conta> contas = repositorioConta.SelecionarTodos();

        Assert.HasCount(2, contas);
        Assert.AreEqual(2, contas.Count(c => c.MesaId == mesa.Id));
    }
}
