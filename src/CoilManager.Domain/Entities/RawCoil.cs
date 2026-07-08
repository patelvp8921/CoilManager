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
        string rawCoilNumber,
        string coilNumber,
        string heatNumber,
        string? poNumber,
        string? invoiceNo,
        string? millTcNo,
        string? bisLicNumber,
        Guid supplierId,
        Guid manufacturerId,
        Guid gradeId,
        decimal thickness,
        string category,
        decimal coreLossPerKg,
        decimal width,
        decimal weight,
        decimal length,
        string? warehouseLocation,
        DateOnly receivedDate,
        CoilStatus status = CoilStatus.Available)
    {
        RawCoilNumber = rawCoilNumber;
        CoilNumber = coilNumber;
        HeatNumber = heatNumber;
        PONumber = poNumber;
        InvoiceNo = invoiceNo;
        MillTCNo = millTcNo;
        BISLicNumber = bisLicNumber;
        SupplierId = supplierId;
        ManufacturerId = manufacturerId;
        GradeId = gradeId;
        Thickness = thickness;
        ThicknessMm = thickness;
        Category = category;
        CoreLossPerKg = coreLossPerKg;
        Width = width;
        Weight = weight;
        Length = length;
        WattLossPerKg = coreLossPerKg;
        WarehouseLocation = warehouseLocation;
        ReceivedDate = receivedDate;
        Status = status;
    }

    public string RawCoilNumber { get; private set; } = string.Empty;
    public string CoilID => RawCoilNumber;
    public string CoilNumber { get; private set; } = string.Empty;
    public string HeatNumber { get; private set; } = string.Empty;
    public string? PONumber { get; private set; }
    public string? InvoiceNo { get; private set; }
    public string? MillTCNo { get; private set; }
    public string? BISLicNumber { get; private set; }
    public Guid SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }
    public Guid ManufacturerId { get; private set; }
    public Manufacturer? Manufacturer { get; private set; }
    public Guid GradeId { get; private set; }
    public Grade? Grade { get; private set; }
    public decimal Thickness { get; private set; }
    public decimal ThicknessMm { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public decimal CoreLossPerKg { get; private set; }
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
        string? poNumber,
        string? invoiceNo,
        string? millTcNo,
        string? bisLicNumber,
        Guid supplierId,
        Guid manufacturerId,
        Guid gradeId,
        decimal thickness,
        string category,
        decimal coreLossPerKg,
        decimal width,
        decimal weight,
        decimal length,
        string? warehouseLocation,
        CoilStatus status,
        DateOnly receivedDate)
    {
        CoilNumber = coilNumber;
        HeatNumber = heatNumber;
        PONumber = poNumber;
        InvoiceNo = invoiceNo;
        MillTCNo = millTcNo;
        BISLicNumber = bisLicNumber;
        SupplierId = supplierId;
        ManufacturerId = manufacturerId;
        GradeId = gradeId;
        Thickness = thickness;
        ThicknessMm = thickness;
        Category = category;
        CoreLossPerKg = coreLossPerKg;
        Width = width;
        Weight = weight;
        Length = length;
        WattLossPerKg = coreLossPerKg;
        WarehouseLocation = warehouseLocation;
        Status = status;
        ReceivedDate = receivedDate;
    }

    public void SetLookupReferences(Supplier supplier, Manufacturer manufacturer, Grade grade)
    {
        Supplier = supplier;
        Manufacturer = manufacturer;
        Grade = grade;
    }
}
