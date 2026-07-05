using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class RawCoilConfiguration : IEntityTypeConfiguration<RawCoil>
{
    public void Configure(EntityTypeBuilder<RawCoil> builder)
    {
        builder.ToTable("RawCoils", "app");

        builder.HasKey(rawCoil => rawCoil.Id);

        builder.Property(rawCoil => rawCoil.CoilID)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(rawCoil => rawCoil.CoilID)
            .IsUnique();

        builder.Property(rawCoil => rawCoil.CoilNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(rawCoil => rawCoil.CoilNumber)
            .IsUnique();

        builder.Property(rawCoil => rawCoil.HeatNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rawCoil => rawCoil.MillName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(rawCoil => rawCoil.MillTCNo)
            .HasMaxLength(100);

        builder.Property(rawCoil => rawCoil.BISLicNumber)
            .HasMaxLength(100);

        builder.Property(rawCoil => rawCoil.SupplierName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(rawCoil => rawCoil.Grade)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rawCoil => rawCoil.Thickness)
            .HasPrecision(18, 3);

        builder.Property(rawCoil => rawCoil.Width)
            .HasPrecision(18, 3);

        builder.Property(rawCoil => rawCoil.Weight)
            .HasPrecision(18, 3);

        builder.Property(rawCoil => rawCoil.Length)
            .HasPrecision(18, 3);

        builder.Property(rawCoil => rawCoil.WattLossPerKg)
            .HasPrecision(18, 4);

        builder.Property(rawCoil => rawCoil.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(rawCoil => rawCoil.WarehouseLocation)
            .HasMaxLength(100);

        builder.Property(rawCoil => rawCoil.CreatedBy)
            .HasMaxLength(100);

        builder.Property(rawCoil => rawCoil.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(rawCoil => rawCoil.DeletedBy)
            .HasMaxLength(100);

        builder.Property(rawCoil => rawCoil.RowVersion)
            .IsRowVersion();
    }
}
