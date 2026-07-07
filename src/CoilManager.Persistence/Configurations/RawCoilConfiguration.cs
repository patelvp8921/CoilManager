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

        builder.Property(rawCoil => rawCoil.RawCoilNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(rawCoil => rawCoil.RawCoilNumber)
            .IsUnique();

        builder.Property(rawCoil => rawCoil.CoilNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(rawCoil => rawCoil.CoilNumber)
            .IsUnique();

        builder.HasIndex(rawCoil => rawCoil.CreatedAtUtc);

        builder.HasIndex(rawCoil => rawCoil.ReceivedDate);

        builder.HasIndex(rawCoil => rawCoil.Status);

        builder.HasIndex(rawCoil => rawCoil.SupplierId);

        builder.HasIndex(rawCoil => rawCoil.ManufacturerId);

        builder.HasIndex(rawCoil => rawCoil.GradeId);

        builder.Property(rawCoil => rawCoil.HeatNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rawCoil => rawCoil.PONumber)
            .HasMaxLength(50);

        builder.Property(rawCoil => rawCoil.InvoiceNo)
            .HasMaxLength(50);

        builder.Property(rawCoil => rawCoil.MillTCNo)
            .HasMaxLength(100);

        builder.Property(rawCoil => rawCoil.BISLicNumber)
            .HasMaxLength(100);

        builder.HasOne(rawCoil => rawCoil.Supplier)
            .WithMany()
            .HasForeignKey(rawCoil => rawCoil.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rawCoil => rawCoil.Manufacturer)
            .WithMany()
            .HasForeignKey(rawCoil => rawCoil.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rawCoil => rawCoil.Grade)
            .WithMany()
            .HasForeignKey(rawCoil => rawCoil.GradeId)
            .OnDelete(DeleteBehavior.Restrict);

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
