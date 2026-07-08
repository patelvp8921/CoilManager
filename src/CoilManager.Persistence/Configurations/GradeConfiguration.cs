using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("Grades", "app");

        builder.HasKey(grade => grade.Id);

        builder.Property(grade => grade.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(grade => grade.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(grade => grade.ThicknessMm)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(grade => grade.Category)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(grade => grade.CoreLossPerKg)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(grade => grade.Description)
            .HasMaxLength(250);

        builder.HasIndex(grade => grade.Code)
            .IsUnique();

        builder.HasIndex(grade => grade.Name);

        builder.Property(grade => grade.CreatedBy)
            .HasMaxLength(100);

        builder.Property(grade => grade.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(grade => grade.RowVersion)
            .IsRowVersion();
    }
}
