using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CoilManager.Persistence.Configurations;
public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "auth"); builder.HasKey(x => x.Id); builder.Property(x => x.Name).HasMaxLength(100).IsRequired(); builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Description).HasMaxLength(500); builder.Property(x => x.CreatedBy).HasMaxLength(100); builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Navigation(x => x.UserRoles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
