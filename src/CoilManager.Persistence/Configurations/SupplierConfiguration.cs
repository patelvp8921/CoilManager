using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers", "app");

        builder.HasKey(supplier => supplier.Id);

        builder.Property(supplier => supplier.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(supplier => supplier.Code)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(supplier => supplier.Code)
            .IsUnique();

        builder.Property(supplier => supplier.CreatedBy)
            .HasMaxLength(100);

        builder.Property(supplier => supplier.UpdatedBy)
            .HasMaxLength(100);
    }
}
