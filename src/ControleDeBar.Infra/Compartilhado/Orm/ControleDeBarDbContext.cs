using System.Reflection;
using ControleDeBar.Dominio.Compartilhado.Identity;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using ControleDeBar.Dominio.Modulos.ModuloPedido;
using ControleDeBar.Dominio.Modulos.ModuloProduto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ControleDeBar.Infra.Compartilhado.Orm;

public sealed class ControleDeBarDbContext(
    DbContextOptions<ControleDeBarDbContext> options,
    IProvedorDeUsuario? userProvider = null
) : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Conta> Contas => Set<Conta>();
    public DbSet<Garcom> Garcons => Set<Garcom>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Assembly assembly = typeof(ControleDeBarDbContext).Assembly;

        modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        if (userProvider is not null)
        {
            modelBuilder.Entity<Conta>()
                .HasQueryFilter(c => c.UserId == userProvider.Id);

            modelBuilder.Entity<Garcom>()
                .HasQueryFilter(g => g.UserId == userProvider.Id);

            modelBuilder.Entity<Mesa>()
                .HasQueryFilter(m => m.UserId == userProvider.Id);

            modelBuilder.Entity<Produto>()
                .HasQueryFilter(p => p.UserId == userProvider.Id);

            modelBuilder.Entity<Pedido>()
                .HasQueryFilter(p => p.UserId == userProvider.Id);
        }
    }

    public override int SaveChanges()
    {
        var entidadesDoUsuario = ChangeTracker
            .Entries<IEntidadeDoUsuario>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entidadesDoUsuario.Count == 0)
            return base.SaveChanges();

        Guid? userId = userProvider?.Id;

        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Não é possível salvar entidades do usuário sem estar autenticado."
            );
        }

        foreach (var entry in entidadesDoUsuario)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.UserId == Guid.Empty)
                    {
                        entry.Property(nameof(IEntidadeDoUsuario.UserId)).CurrentValue = userId.Value;
                    }
                    else if (entry.Entity.UserId != userId.Value)
                    {
                        throw new UnauthorizedAccessException(
                            "Tentativa de criar entidade para outro usuário."
                        );
                    }

                    break;

                case EntityState.Modified:
                    Guid idOriginalUsuario = entry
                        .Property(nameof(IEntidadeDoUsuario.UserId))
                        .OriginalValue is Guid idOriginal
                        ? idOriginal
                        : Guid.Empty;

                    Guid idAtualUsuario = entry
                        .Property(nameof(IEntidadeDoUsuario.UserId))
                        .CurrentValue is Guid idAtual
                        ? idAtual
                        : Guid.Empty;

                    if (idOriginalUsuario != idAtualUsuario)
                    {
                        throw new UnauthorizedAccessException(
                              "Não é permitido alterar o usuário de uma entidade."
                          );
                    }

                    if (idAtualUsuario != userId.Value)
                    {
                        throw new UnauthorizedAccessException(
                            "Tentativa de modificar entidade de outro usuário."
                        );
                    }

                    break;

                case EntityState.Deleted:
                    Guid usuarioOriginal = entry
                        .Property(nameof(IEntidadeDoUsuario.UserId))
                        .OriginalValue is Guid original
                        ? original
                        : Guid.Empty;

                    if (usuarioOriginal != userId.Value)
                    {
                        throw new UnauthorizedAccessException(
                            "Tentativa de excluir entidade de outro usuário."
                        );
                    }

                    break;
            }
        }

        return base.SaveChanges();
    }
}
