using ControleDeBar.Aplicacao.Modulos.ModuloConta;
using ControleDeBar.Aplicacao.Modulos.ModuloPedido;
using ControleDeBar.Aplicacao.Modulos.ModuloProduto;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using ControleDeBar.Dominio.Modulos.ModuloPedido;
using ControleDeBar.Dominio.Modulos.ModuloProduto;
using ControleDeBar.Infra.Compartilhado.Orm;
using ControleDeBar.Infra.Modulos.ModuloConta;
using ControleDeBar.Infra.Modulos.ModuloGarcom;
using ControleDeBar.Infra.Modulos.ModuloMesa;
using ControleDeBar.Infra.Modulos.ModuloPedido;
using ControleDeBar.Infra.Modulos.ModuloProduto;
using ControleDeBar.Testes.Integracao.Compartilhado.Orm;
using FluentResults;

namespace ControleDeBar.Testes.Integracao.Compartilhado.Identity;

[TestClass]
public sealed class AutorizacaoMultiTenancyTests : RepositorioBaseEmOrmTests
{
    private RepositorioContaEmOrm repositorioConta = null!;
    private RepositorioPedidoEmOrm repositorioPedido = null!;
    private RepositorioProdutoEmOrm repositorioProduto = null!;

    [TestInitialize]
    public override void InicializarContexto()
    {
        base.InicializarContexto();

        repositorioConta = new RepositorioContaEmOrm(dbContext);
        repositorioPedido = new RepositorioPedidoEmOrm(dbContext);
        repositorioProduto = new RepositorioProdutoEmOrm(dbContext);
    }

    [TestMethod]
    public void Deve_RejeitarAbertura_DeConta_ComMesaDeOutroUsuario()
    {
        // Arrange
        Garcom garcom = new("Marcos");
        repositorioGarcom.Cadastrar(garcom);

        using ControleDeBarDbContext outroContexto = CriarDbContext(Guid.CreateVersion7());
        RepositorioMesaEmOrm outroRepositorioMesa = new(outroContexto);
        Mesa mesaDeOutroUsuario = new(1, 4);
        outroRepositorioMesa.Cadastrar(mesaDeOutroUsuario);

        ServicoConta servicoConta = new(repositorioConta, repositorioMesa, repositorioGarcom);
        AbrirContaDto dto = new(mesaDeOutroUsuario.Id, garcom.Id, "Carlos");

        // Act
        Result resultado = servicoConta.Abrir(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Mesa não encontrada.", resultado.Errors[0].Message);
        Assert.IsEmpty(repositorioConta.SelecionarTodos());
        Assert.IsNotNull(outroRepositorioMesa.SelecionarPorId(mesaDeOutroUsuario.Id));
    }

    [TestMethod]
    public void Deve_RejeitarAbertura_DeConta_ComGarcomDeOutroUsuario()
    {
        // Arrange
        Mesa mesa = new(1, 4);
        repositorioMesa.Cadastrar(mesa);

        using ControleDeBarDbContext outroContexto = CriarDbContext(Guid.CreateVersion7());
        RepositorioGarcomEmOrm outroRepositorioGarcom = new(outroContexto);
        Garcom garcomDeOutroUsuario = new("Marcos");
        outroRepositorioGarcom.Cadastrar(garcomDeOutroUsuario);

        ServicoConta servicoConta = new(repositorioConta, repositorioMesa, repositorioGarcom);
        AbrirContaDto dto = new(mesa.Id, garcomDeOutroUsuario.Id, "Carlos");

        // Act
        Result resultado = servicoConta.Abrir(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Garçom não encontrado.", resultado.Errors[0].Message);
        Assert.IsEmpty(repositorioConta.SelecionarTodos());
        Assert.AreEqual(StatusMesa.Livre, repositorioMesa.SelecionarPorId(mesa.Id)!.Status);
        Assert.IsNotNull(outroRepositorioGarcom.SelecionarPorId(garcomDeOutroUsuario.Id));
    }

    [TestMethod]
    public void Deve_RejeitarAdicao_DePedido_ComProdutoDeOutroUsuario()
    {
        // Arrange
        Mesa mesa = new(1, 4);
        Garcom garcom = new("Marcos");
        repositorioMesa.Cadastrar(mesa);
        repositorioGarcom.Cadastrar(garcom);

        Conta conta = new(mesa.Id, garcom.Id, "Carlos");
        repositorioConta.Cadastrar(conta);

        using ControleDeBarDbContext outroContexto = CriarDbContext(Guid.CreateVersion7());
        RepositorioProdutoEmOrm outroRepositorioProduto = new(outroContexto);
        Produto produtoDeOutroUsuario = new("Cerveja", 8.50m);
        outroRepositorioProduto.Cadastrar(produtoDeOutroUsuario);

        ServicoPedido servicoPedido = new(repositorioPedido, repositorioConta, repositorioProduto);
        AdicionarPedidoDto dto = new(conta.Id, produtoDeOutroUsuario.Id, 2);

        // Act
        Result resultado = servicoPedido.Adicionar(dto);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Produto não encontrado.", resultado.Errors[0].Message);
        Assert.IsEmpty(repositorioPedido.SelecionarTodos());
        Assert.IsNotNull(outroRepositorioProduto.SelecionarPorId(produtoDeOutroUsuario.Id));
    }

    [TestMethod]
    public void Deve_RejeitarExclusao_DeProdutoDeOutroUsuario()
    {
        // Arrange
        using ControleDeBarDbContext outroContexto = CriarDbContext(Guid.CreateVersion7());
        RepositorioProdutoEmOrm outroRepositorioProduto = new(outroContexto);
        Produto produtoDeOutroUsuario = new("Cerveja", 8.50m);
        outroRepositorioProduto.Cadastrar(produtoDeOutroUsuario);

        ServicoProduto servicoProduto = new(repositorioProduto, repositorioPedido);

        // Act
        Result resultado = servicoProduto.Excluir(produtoDeOutroUsuario.Id);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Produto não encontrado.", resultado.Errors[0].Message);
        Assert.IsNotNull(outroRepositorioProduto.SelecionarPorId(produtoDeOutroUsuario.Id));
    }

    [TestMethod]
    public void Deve_RejeitarFechamento_DeContaDeOutroUsuario()
    {
        // Arrange
        using ControleDeBarDbContext outroContexto = CriarDbContext(Guid.CreateVersion7());
        RepositorioMesaEmOrm outroRepositorioMesa = new(outroContexto);
        RepositorioGarcomEmOrm outroRepositorioGarcom = new(outroContexto);
        RepositorioContaEmOrm outroRepositorioConta = new(outroContexto);

        Mesa mesa = new(1, 4);
        Garcom garcom = new("Marcos");
        outroRepositorioMesa.Cadastrar(mesa);
        outroRepositorioGarcom.Cadastrar(garcom);

        Conta contaDeOutroUsuario = new(mesa.Id, garcom.Id, "Carlos");
        outroRepositorioConta.Cadastrar(contaDeOutroUsuario);

        ServicoConta servicoConta = new(repositorioConta, repositorioMesa, repositorioGarcom);

        // Act
        Result resultado = servicoConta.Fechar(contaDeOutroUsuario.Id);

        // Assert
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Conta não encontrada.", resultado.Errors[0].Message);

        outroContexto.ChangeTracker.Clear();
        Conta? contaPersistida = outroRepositorioConta.SelecionarPorId(contaDeOutroUsuario.Id);

        Assert.IsNotNull(contaPersistida);
        Assert.AreEqual(StatusConta.Aberta, contaPersistida.Status);
        Assert.IsNull(contaPersistida.DataFechamento);
    }
}
