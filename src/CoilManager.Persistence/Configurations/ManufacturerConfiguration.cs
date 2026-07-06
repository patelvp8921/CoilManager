using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        builder.ToTable("Manufacturers", "app");

        builder.HasKey(manufacturer => manufacturer.Id);

        builder.Property(manufacturer => manufacturer.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(manufacturer => manufacturer.Code)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(manufacturer => manufacturer.Code)
            .IsUnique();

        builder.Property(manufacturer => manufacturer.CreatedBy)
            .HasMaxLength(100);

        builder.Property(manufacturer => manufacturer.UpdatedBy)
            .HasMaxLength(100);
    }
}
