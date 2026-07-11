using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;

namespace CoilManager.Domain.Entities;

public sealed class InventoryTransaction : AuditableEntity
{
    private InventoryTransaction()
    {
    }

    public InventoryTransaction(
        InventoryTransactionType transactionType,
        CoilType coilType,
        Guid coilId,
        string coilNumber,
        Guid? relatedDocumentId,
        string? relatedDocumentNumber,
        CoilStatus? fromStatus,
        CoilStatus toStatus,
        decimal quantityWeight,
        DateTimeOffset transactionDate,
        string? remarks)
    {
        TransactionType = transactionType;
        CoilType = coilType;
        CoilId = coilId;
        CoilNumber = coilNumber;
        RelatedDocumentId = relatedDocumentId;
        RelatedDocumentNumber = relatedDocumentNumber;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        QuantityWeight = quantityWeight;
        TransactionDate = transactionDate;
        Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
    }

    public InventoryTransactionType TransactionType { get; private set; }
    public CoilType CoilType { get; private set; }
    public Guid CoilId { get; private set; }
    public string CoilNumber { get; private set; } = string.Empty;
    public Guid? RelatedDocumentId { get; private set; }
    public string? RelatedDocumentNumber { get; private set; }
    public CoilStatus? FromStatus { get; private set; }
    public CoilStatus ToStatus { get; private set; }
    public decimal QuantityWeight { get; private set; }
    public DateTimeOffset TransactionDate { get; private set; }
    public string? Remarks { get; private set; }
}
