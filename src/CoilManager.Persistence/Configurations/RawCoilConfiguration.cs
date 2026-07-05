using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class RawCoilConfiguration : IEntityTypeConfiguration<RawCoil>
{
    public void Configure(EntityTypeBuilder<RawCoil> builder)
    {
        builder.ToTable("RawCoils", "app");

        builder.HasKey(rawCoil => rawCoil.Id);

        builder.Property(rawCoil => rawCoil.CoilNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(rawCoil => rawCoil.CoilNumber)
            .IsUnique();

        builder.Property(rawCoil => rawCoil.HeatNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rawCoil => rawCoil.SupplierName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(rawCoil => rawCoil.Grade)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rawCoil => rawCoil.ThicknessMm)
            .HasPrecision(18, 3);

        builder.Property(rawCoil => rawCoil.WidthMm)
            .HasPrecision(18, 3);

        builder.Property(rawCoil => rawCoil.WeightMt)
            .HasPrecision(18, 3);

        builder.Property(rawCoil => rawCoil.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(rawCoil => rawCoil.Warehouse)
            .HasMaxLength(100);

        builder.Property(rawCoil => rawCoil.Location)
            .HasMaxLength(100);

        builder.Property(rawCoil => rawCoil.CreatedBy)
            .HasMaxLength(100);

        builder.Property(rawCoil => rawCoil.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(rawCoil => rawCoil.DeletedBy)
            .HasMaxLength(100);

        builder.HasQueryFilter(rawCoil => !rawCoil.IsDeleted);
    }
}
