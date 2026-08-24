using ControleDeBar.Dominio.Modulos.ModuloProduto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeBar.Infra.Modulos.ModuloProduto;

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("TBProduto");

        builder.HasKey(p => p.Id)
            .HasName("PK_TBProduto");

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.Nome)
            .IsRequired();

        builder.Property(p => p.Preco)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.HasIndex(p => new { p.UserId, p.Nome })
            .IsUnique()
            .HasDatabaseName("UQ_TBProduto_UserId_Nome");
    }
}
