using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class SlitCoilLabelPrintHistoryConfiguration : IEntityTypeConfiguration<SlitCoilLabelPrintHistory>
{
    public void Configure(EntityTypeBuilder<SlitCoilLabelPrintHistory> builder)
    {
        builder.ToTable("SlitCoilLabelPrintHistories", "app");
        builder.HasKey(row => row.Id);
        builder.Property(row => row.CoilNumber).HasMaxLength(60).IsRequired();
        builder.Property(row => row.LabelVersion).HasMaxLength(20).IsRequired();
        builder.Property(row => row.PrintedBy).HasMaxLength(100);
        builder.Property(row => row.PrinterName).HasMaxLength(150);
        builder.Property(row => row.Remarks).HasMaxLength(500);
        builder.Property(row => row.PrintType).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(row => row.SlitCoilId);
        builder.HasIndex(row => row.CoilNumber);
        builder.HasIndex(row => row.PrintedOn);
        builder.HasOne(row => row.SlitCoil).WithMany().HasForeignKey(row => row.SlitCoilId).OnDelete(DeleteBehavior.Restrict);
    }
}
