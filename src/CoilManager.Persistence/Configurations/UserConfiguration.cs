using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CoilManager.Persistence.Configurations;
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "auth"); builder.HasKey(x => x.Id);
        builder.Property(x => x.UserName).HasMaxLength(100).IsRequired(); builder.HasIndex(x => x.UserName).IsUnique();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired(); builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.CreatedBy).HasMaxLength(100); builder.Property(x => x.UpdatedBy).HasMaxLength(100); builder.Property(x => x.DeletedBy).HasMaxLength(100);
        builder.Navigation(x => x.UserRoles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
