using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoilManager.Domain.Entities;

public sealed class RawCoil : SoftDeletableEntity
{
    private RawCoil()
    {
    }

    public RawCoil(
        string coilId,
        string coilNumber,
        string heatNumber,
        string millName,
        string? millTcNo,
        string? bisLicNumber,
        string supplierName,
        string grade,
        decimal thickness,
        decimal width,
        decimal weight,
        decimal length,
        decimal wattLossPerKg,
        string? warehouseLocation,
        DateOnly receivedDate)
    {
        CoilID = coilId;
        CoilNumber = coilNumber;
        HeatNumber = heatNumber;
        MillName = millName;
        MillTCNo = millTcNo;
        BISLicNumber = bisLicNumber;
        SupplierName = supplierName;
        Grade = grade;
        Thickness = thickness;
        Width = width;
        Weight = weight;
        Length = length;
        WattLossPerKg = wattLossPerKg;
        WarehouseLocation = warehouseLocation;
        ReceivedDate = receivedDate;
        Status = CoilStatus.Available;
    }

    public string CoilID { get; private set; } = string.Empty;
    public string CoilNumber { get; private set; } = string.Empty;
    public string HeatNumber { get; private set; } = string.Empty;
    public string MillName { get; private set; } = string.Empty;
    public string? MillTCNo { get; private set; }
    public string? BISLicNumber { get; private set; }
    public string SupplierName { get; private set; } = string.Empty;
    public string Grade { get; private set; } = string.Empty;
    public decimal Thickness { get; private set; }
    public decimal Width { get; private set; }
    public decimal Weight { get; private set; }
    public decimal Length { get; private set; }
    public decimal WattLossPerKg { get; private set; }
    public string? WarehouseLocation { get; private set; }
    public CoilStatus Status { get; private set; }
    public DateOnly ReceivedDate { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    [NotMapped]
    public DateTimeOffset CreatedOn => CreatedAtUtc;

    [NotMapped]
    public DateTimeOffset? ModifiedOn => UpdatedAtUtc;

    [NotMapped]
    public string? ModifiedBy => UpdatedBy;

    [NotMapped]
    public DateTimeOffset? DeletedOn => DeletedAtUtc;

    public void Update(
        string coilNumber,
        string heatNumber,
        string millName,
        string? millTcNo,
        string? bisLicNumber,
        string supplierName,
        string grade,
        decimal thickness,
        decimal width,
        decimal weight,
        decimal length,
        decimal wattLossPerKg,
        string? warehouseLocation,
        CoilStatus status,
        DateOnly receivedDate)
    {
        CoilNumber = coilNumber;
        HeatNumber = heatNumber;
        MillName = millName;
        MillTCNo = millTcNo;
        BISLicNumber = bisLicNumber;
        SupplierName = supplierName;
        Grade = grade;
        Thickness = thickness;
        Width = width;
        Weight = weight;
        Length = length;
        WattLossPerKg = wattLossPerKg;
        WarehouseLocation = warehouseLocation;
        Status = status;
        ReceivedDate = receivedDate;
    }
}
