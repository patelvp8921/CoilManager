using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;

namespace CoilManager.Domain.Entities;

public sealed class RawCoil : SoftDeletableEntity
{
    private RawCoil()
    {
    }

    public RawCoil(
        string coilNumber,
        string heatNumber,
        string supplierName,
        string grade,
        decimal thicknessMm,
        decimal widthMm,
        decimal weightMt,
        DateOnly receivedDate)
    {
        CoilNumber = coilNumber;
        HeatNumber = heatNumber;
        SupplierName = supplierName;
        Grade = grade;
        ThicknessMm = thicknessMm;
        WidthMm = widthMm;
        WeightMt = weightMt;
        ReceivedDate = receivedDate;
        Status = CoilStatus.Available;
    }

    public string CoilNumber { get; private set; } = string.Empty;
    public string HeatNumber { get; private set; } = string.Empty;
    public string SupplierName { get; private set; } = string.Empty;
    public string Grade { get; private set; } = string.Empty;
    public decimal ThicknessMm { get; private set; }
    public decimal WidthMm { get; private set; }
    public decimal WeightMt { get; private set; }
    public CoilStatus Status { get; private set; }
    public string? Warehouse { get; private set; }
    public string? Location { get; private set; }
    public DateOnly ReceivedDate { get; private set; }
}
