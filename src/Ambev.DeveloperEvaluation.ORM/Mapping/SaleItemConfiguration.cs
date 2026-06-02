using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnType("uuid");
        builder.Property(i => i.SaleId).HasColumnType("uuid");
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.DiscountPercent).HasPrecision(5, 2);
        builder.Property(i => i.LineTotal).HasPrecision(18, 2);
        builder.Property(i => i.IsCancelled).IsRequired();

        builder.OwnsOne(i => i.Product, product =>
        {
            product.Property(p => p.Id).HasColumnName("ProductId").HasColumnType("uuid");
            product.Property(p => p.Description).HasColumnName("ProductName").HasMaxLength(200);
        });
    }
}
