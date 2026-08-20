using ControleDeBar.Infra.Compartilhado.Orm;
using ControleDeBar.Infra.Modulos.ModuloMesa;
using ControleDeBar.Testes.Integracao.Compartilhado.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ControleDeBar.Testes.Integracao.Compartilhado.Orm;

public abstract class RepositorioBaseEmOrmTests
{
    private string nomeBanco = null!;
    private InMemoryDatabaseRoot raizBanco = null!;

    protected Guid userId;
    protected ControleDeBarDbContext dbContext = null!;
    protected RepositorioMesaEmOrm repositorioMesa = null!;

    [TestInitialize]
    public void InicializarContexto()
    {
        nomeBanco = $"integracao-{Guid.NewGuid():N}";
        raizBanco = new InMemoryDatabaseRoot();
        userId = Guid.CreateVersion7();

        dbContext = CriarDbContext(userId);
        repositorioMesa = new RepositorioMesaEmOrm(dbContext);
    }

    [TestCleanup]
    public void DescartarContexto()
    {
        dbContext.Dispose();
    }

    protected ControleDeBarDbContext CriarDbContext(Guid idUsuario)
    {
        DbContextOptions<ControleDeBarDbContext> options =
            new DbContextOptionsBuilder<ControleDeBarDbContext>()
                .UseInMemoryDatabase(nomeBanco, raizBanco)
                .Options;

        return new ControleDeBarDbContext(
            options,
            new ProvedorDeUsuarioFake(idUsuario)
        );
    }
}
