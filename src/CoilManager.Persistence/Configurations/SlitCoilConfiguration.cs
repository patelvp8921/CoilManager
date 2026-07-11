using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class SlitCoilConfiguration : IEntityTypeConfiguration<SlitCoil>
{
    public void Configure(EntityTypeBuilder<SlitCoil> builder)
    {
        builder.ToTable("SlitCoils", "app");

        builder.HasKey(coil => coil.Id);

        builder.Property(coil => coil.CoilNumber)
            .HasMaxLength(60)
            .IsRequired();

        builder.HasIndex(coil => coil.CoilNumber)
            .IsUnique();

        builder.HasIndex(coil => coil.MotherCoilId);
        builder.HasIndex(coil => coil.RootMotherCoilId);
        builder.HasIndex(coil => coil.ParentCoilId);
        builder.HasIndex(coil => coil.SlittingJobId);
        builder.HasIndex(coil => coil.Status);

        builder.Property(coil => coil.HeatNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(coil => coil.Thickness)
            .HasPrecision(18, 3);

        builder.Property(coil => coil.Category)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(coil => coil.CoreLossPerKg)
            .HasPrecision(18, 4);

        builder.Property(coil => coil.Width)
            .HasPrecision(18, 3);

        builder.Property(coil => coil.Weight)
            .HasPrecision(18, 3);

        builder.Property(coil => coil.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(coil => coil.WarehouseLocation)
            .HasMaxLength(100);

        builder.Property(coil => coil.BarcodeValue)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(coil => coil.QrCodeValue)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(coil => coil.LabelVersion)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(coil => coil.CreatedBy)
            .HasMaxLength(100);

        builder.Property(coil => coil.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(coil => coil.DeletedBy)
            .HasMaxLength(100);

        builder.Property(coil => coil.RowVersion)
            .IsRowVersion();

        builder.HasOne(coil => coil.MotherCoil)
            .WithMany()
            .HasForeignKey(coil => coil.MotherCoilId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(coil => coil.SlittingJob)
            .WithMany()
            .HasForeignKey(coil => coil.SlittingJobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(coil => coil.Supplier)
            .WithMany()
            .HasForeignKey(coil => coil.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(coil => coil.Manufacturer)
            .WithMany()
            .HasForeignKey(coil => coil.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(coil => coil.Grade)
            .WithMany()
            .HasForeignKey(coil => coil.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
