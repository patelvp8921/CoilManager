using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions", "app");

        builder.HasKey(transaction => transaction.Id);

        builder.HasIndex(transaction => transaction.CoilId);
        builder.HasIndex(transaction => transaction.CoilNumber);
        builder.HasIndex(transaction => transaction.RelatedDocumentId);
        builder.HasIndex(transaction => transaction.TransactionDate);

        builder.Property(transaction => transaction.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(transaction => transaction.CoilType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(transaction => transaction.CoilNumber)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(transaction => transaction.RelatedDocumentNumber)
            .HasMaxLength(60);

        builder.Property(transaction => transaction.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(transaction => transaction.ToStatus)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(transaction => transaction.QuantityWeight)
            .HasPrecision(18, 3);

        builder.Property(transaction => transaction.Remarks)
            .HasMaxLength(250);

        builder.Property(transaction => transaction.CreatedBy)
            .HasMaxLength(100);
    }
}
