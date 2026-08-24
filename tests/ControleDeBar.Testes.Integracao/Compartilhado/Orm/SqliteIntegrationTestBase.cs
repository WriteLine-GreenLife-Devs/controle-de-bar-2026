using ControleDeBar.Infra.Compartilhado.Orm;
using ControleDeBar.Testes.Integracao.Compartilhado.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ControleDeBar.Testes.Integracao.Compartilhado.Orm;

public abstract class SqliteIntegrationTestBase
{
    protected Guid userId;
    protected SqliteConnection connection = null!;
    protected ControleDeBarDbContext dbContext = null!;

    [TestInitialize]
    public void InicializarSqlite()
    {
        userId = Guid.CreateVersion7();
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();
        }

        DbContextOptions<ControleDeBarDbContext> options =
            new DbContextOptionsBuilder<ControleDeBarDbContext>()
                .UseSqlite(connection)
                .Options;

        dbContext = new ControleDeBarDbContext(
            options,
            new ProvedorDeUsuarioFake(userId)
        );

        dbContext.Database.EnsureCreated();

        using SqliteCommand pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = "PRAGMA foreign_keys;";
        int foreignKeysEnabled = Convert.ToInt32(pragmaCommand.ExecuteScalar());

        if (foreignKeysEnabled != 1)
            throw new InvalidOperationException("SQLite foreign keys não estão habilitadas.");
    }

    [TestCleanup]
    public void DescartarSqlite()
    {
        dbContext.Dispose();
        connection.Dispose();
    }
}
