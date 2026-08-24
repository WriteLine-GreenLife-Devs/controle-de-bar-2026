using ControleDeBar.Dominio.Modulos.ModuloConta;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeBar.Infra.Modulos.ModuloConta;

public sealed class ContaConfiguration : IEntityTypeConfiguration<Conta>
{
    public void Configure(EntityTypeBuilder<Conta> builder)
    {
        builder.ToTable("TBConta");

        builder.HasKey(c => c.Id)
            .HasName("PK_TBConta");

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.MesaId)
            .IsRequired();

        builder.Property(c => c.GarcomId)
            .IsRequired();

        builder.Property(c => c.NomeCliente)
            .IsRequired();

        builder.Property(c => c.DataAbertura)
            .IsRequired();

        builder.Property(c => c.DataFechamento)
            .IsRequired(false);

        builder.Property(c => c.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(c => c.Mesa)
            .WithMany()
            .HasForeignKey(c => c.MesaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Garcom)
            .WithMany()
            .HasForeignKey(c => c.GarcomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
