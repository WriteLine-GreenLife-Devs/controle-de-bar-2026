using ControleDeBar.Dominio.Modulos.ModuloPedido;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeBar.Infra.Modulos.ModuloPedido;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("TBPedido");

        builder.HasKey(p => p.Id)
            .HasName("PK_TBPedido");

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.ContaId)
            .IsRequired();

        builder.Property(p => p.ProdutoId)
            .IsRequired();

        builder.Property(p => p.NomeProduto)
            .IsRequired();

        builder.Property(p => p.PrecoPraticado)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(p => p.Quantidade)
            .IsRequired();

        builder.Ignore(p => p.Subtotal);

        builder.HasOne(p => p.Conta)
            .WithMany()
            .HasForeignKey(p => p.ContaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Produto)
            .WithMany()
            .HasForeignKey(p => p.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
