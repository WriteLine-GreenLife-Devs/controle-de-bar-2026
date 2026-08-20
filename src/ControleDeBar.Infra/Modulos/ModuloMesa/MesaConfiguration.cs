using ControleDeBar.Dominio.Modulos.ModuloMesa;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeBar.Infra.Modulos.ModuloMesa;

public sealed class MesaConfiguration : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> builder)
    {
        builder.ToTable("TBMesa");

        builder.HasKey(m => m.Id)
            .HasName("PK_TBMesa");

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.UserId)
            .IsRequired();

        builder.Property(m => m.Numero)
            .IsRequired();

        builder.Property(m => m.Lugares)
            .IsRequired();

        builder.Property(m => m.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(m => new { m.UserId, m.Numero })
            .IsUnique()
            .HasDatabaseName("UQ_TBMesa_UserId_Numero");
    }
}
