using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnType("uuid");

        builder.Property(s => s.SaleNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.SaleNumber).IsUnique();

        builder.Property(s => s.SaleDate).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.TotalAmount).HasPrecision(18, 2);
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.OwnsOne(s => s.Customer, customer =>
        {
            customer.Property(c => c.Id).HasColumnName("CustomerId").HasColumnType("uuid");
            customer.Property(c => c.Description).HasColumnName("CustomerName").HasMaxLength(200);
        });

        builder.OwnsOne(s => s.Branch, branch =>
        {
            branch.Property(b => b.Id).HasColumnName("BranchId").HasColumnType("uuid");
            branch.Property(b => b.Description).HasColumnName("BranchName").HasMaxLength(200);
        });

        builder.HasMany<SaleItem>("_items")
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(s => s.Items);
        builder.Ignore(s => s.DomainEvents);
    }
}
