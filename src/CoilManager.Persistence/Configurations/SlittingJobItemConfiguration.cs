using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class SlittingJobItemConfiguration : IEntityTypeConfiguration<SlittingJobItem>
{
    public void Configure(EntityTypeBuilder<SlittingJobItem> builder)
    {
        builder.ToTable("SlittingJobItems", "app");

        builder.HasKey(item => item.Id);

        builder.HasIndex(item => new { item.SlittingJobId, item.SequenceNo })
            .IsUnique();

        builder.Property(item => item.SlitCoilId)
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(item => item.SlitCoilId)
            .IsUnique();

        builder.Property(item => item.Width)
            .HasPrecision(18, 3);

        builder.Property(item => item.EstimatedWeight)
            .HasPrecision(18, 3);

        builder.Property(item => item.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(item => item.Remarks)
            .HasMaxLength(250);
    }
}
