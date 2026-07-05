using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "auth");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.Property(role => role.Description)
            .HasMaxLength(500);

        builder.Property(role => role.CreatedBy)
            .HasMaxLength(100);

        builder.Property(role => role.UpdatedBy)
            .HasMaxLength(100);

        builder.Navigation(role => role.UserRoles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
