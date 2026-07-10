using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class SlittingJobConfiguration : IEntityTypeConfiguration<SlittingJob>
{
    public void Configure(EntityTypeBuilder<SlittingJob> builder)
    {
        builder.ToTable("SlittingJobs", "app");

        builder.HasKey(job => job.Id);

        builder.Property(job => job.SlittingJobNo)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(job => job.SlittingJobNo)
            .IsUnique();

        builder.HasIndex(job => job.PlanningDate);
        builder.HasIndex(job => job.Status);
        builder.HasIndex(job => job.MotherCoilId);

        builder.Property(job => job.PlannerId)
            .HasMaxLength(100);

        builder.Property(job => job.Shift)
            .HasMaxLength(30);

        builder.Property(job => job.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(job => job.KnifeThickness)
            .HasPrecision(18, 3);

        builder.Property(job => job.LeftEdgeTrim)
            .HasPrecision(18, 3);

        builder.Property(job => job.RightEdgeTrim)
            .HasPrecision(18, 3);

        builder.Property(job => job.Remarks)
            .HasMaxLength(500);

        builder.Property(job => job.CreatedBy)
            .HasMaxLength(100);

        builder.Property(job => job.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(job => job.RowVersion)
            .IsRowVersion();

        builder.HasOne(job => job.MotherCoil)
            .WithMany()
            .HasForeignKey(job => job.MotherCoilId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(job => job.Items)
            .WithOne(item => item.SlittingJob)
            .HasForeignKey(item => item.SlittingJobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(job => job.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
