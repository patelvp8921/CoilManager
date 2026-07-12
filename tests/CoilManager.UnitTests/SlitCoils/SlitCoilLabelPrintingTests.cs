using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;

namespace CoilManager.UnitTests.SlitCoils;

public sealed class SlitCoilLabelPrintingTests
{
    [Fact]
    public void FirstPrint_SetsPrintedAndCountWithoutChangingVersionOrStatus()
    {
        SlitCoil coil = CreateCoil();
        CoilStatus status = coil.Status;
        LabelPrintType type = coil.RecordLabelPrint(DateTimeOffset.UtcNow, "Admin");
        Assert.Equal(LabelPrintType.Initial, type);
        Assert.True(coil.LabelPrinted);
        Assert.Equal(1, coil.LabelPrintCount);
        Assert.Equal("1", coil.LabelVersion);
        Assert.Equal(status, coil.Status);
    }

    [Fact]
    public void Reprint_IncrementsCountButNotVersion()
    {
        SlitCoil coil = CreateCoil();
        coil.RecordLabelPrint(DateTimeOffset.UtcNow, "Admin");
        LabelPrintType type = coil.RecordLabelPrint(DateTimeOffset.UtcNow.AddMinutes(1), "Admin");
        Assert.Equal(LabelPrintType.Reprint, type);
        Assert.Equal(2, coil.LabelPrintCount);
        Assert.Equal("1", coil.LabelVersion);
    }

    [Fact]
    public void IncrementVersion_ChangesOnlyLabelVersion()
    {
        SlitCoil coil = CreateCoil();
        CoilStatus status = coil.Status;
        coil.IncrementLabelVersion();
        Assert.Equal("2", coil.LabelVersion);
        Assert.Equal(0, coil.LabelPrintCount);
        Assert.Equal(status, coil.Status);
    }

    [Fact]
    public void Print_EnforcesCoilNumberAsBarcodeAndQrPayload()
    {
        SlitCoil coil = CreateCoil();
        coil.RecordLabelPrint(DateTimeOffset.UtcNow, "Admin");
        Assert.Equal(coil.CoilNumber, coil.BarcodeValue);
        Assert.Equal(coil.CoilNumber, coil.QrCodeValue);
    }

    [Fact]
    public void History_CapturesPrintMetadata()
    {
        SlitCoil coil = CreateCoil();
        var history = new SlitCoilLabelPrintHistory(coil.Id, coil.CoilNumber, coil.LabelVersion,
            "Admin", DateTimeOffset.UtcNow, 2, "Thermal-01", LabelPrintType.BatchPrint, "Production batch");
        Assert.Equal(2, history.Copies);
        Assert.Equal(LabelPrintType.BatchPrint, history.PrintType);
        Assert.Equal("Thermal-01", history.PrinterName);
    }

    private static SlitCoil CreateCoil() => new("SC-2026-00001-03", Guid.NewGuid(), Guid.NewGuid(),
        Guid.NewGuid(), Guid.NewGuid(), 3, 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        "HN240315", 0.23m, "M3", 0.85m, 120m, 415.6m, "A-01", "1");
}
