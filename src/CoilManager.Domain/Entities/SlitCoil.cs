using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;

namespace CoilManager.Domain.Entities;

public sealed class SlitCoil : SoftDeletableEntity
{
    private SlitCoil()
    {
    }

    public SlitCoil(
        string coilNumber,
        Guid parentCoilId,
        Guid rootMotherCoilId,
        Guid motherCoilId,
        Guid slittingJobId,
        int slitSequence,
        int generationLevel,
        Guid gradeId,
        Guid supplierId,
        Guid manufacturerId,
        string heatNumber,
        decimal thickness,
        string category,
        decimal coreLossPerKg,
        decimal width,
        decimal weight,
        string? warehouseLocation,
        string labelVersion)
    {
        CoilNumber = coilNumber;
        ParentCoilId = parentCoilId;
        RootMotherCoilId = rootMotherCoilId;
        MotherCoilId = motherCoilId;
        SlittingJobId = slittingJobId;
        SlitSequence = slitSequence;
        GenerationLevel = generationLevel;
        GradeId = gradeId;
        SupplierId = supplierId;
        ManufacturerId = manufacturerId;
        HeatNumber = heatNumber;
        Thickness = thickness;
        Category = category;
        CoreLossPerKg = coreLossPerKg;
        Width = width;
        Weight = weight;
        WarehouseLocation = warehouseLocation;
        Status = CoilStatus.Available;
        BarcodeValue = coilNumber;
        QrCodeValue = coilNumber;
        LabelVersion = labelVersion;
    }

    public string CoilNumber { get; private set; } = string.Empty;
    public Guid ParentCoilId { get; private set; }
    public Guid RootMotherCoilId { get; private set; }
    public Guid MotherCoilId { get; private set; }
    public Guid SlittingJobId { get; private set; }
    public int SlitSequence { get; private set; }
    public int GenerationLevel { get; private set; }
    public Guid GradeId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid ManufacturerId { get; private set; }
    public string HeatNumber { get; private set; } = string.Empty;
    public decimal Thickness { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public decimal CoreLossPerKg { get; private set; }
    public decimal Width { get; private set; }
    public decimal Weight { get; private set; }
    public string? WarehouseLocation { get; private set; }
    public CoilStatus Status { get; private set; }
    public string BarcodeValue { get; private set; } = string.Empty;
    public string QrCodeValue { get; private set; } = string.Empty;
    public string LabelVersion { get; private set; } = "1";
    public byte[] RowVersion { get; private set; } = [];

    public RawCoil? MotherCoil { get; private set; }
    public SlittingJob? SlittingJob { get; private set; }
    public Grade? Grade { get; private set; }
    public Supplier? Supplier { get; private set; }
    public Manufacturer? Manufacturer { get; private set; }
}
