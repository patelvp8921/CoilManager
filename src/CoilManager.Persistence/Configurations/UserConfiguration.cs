using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "auth");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.UserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(user => user.UserName)
            .IsUnique();

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.CreatedBy)
            .HasMaxLength(100);

        builder.Property(user => user.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(user => user.DeletedBy)
            .HasMaxLength(100);

        builder.Navigation(user => user.UserRoles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
