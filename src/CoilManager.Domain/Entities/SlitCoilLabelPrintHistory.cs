using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;

namespace CoilManager.Domain.Entities;

public sealed class SlitCoilLabelPrintHistory : AuditableEntity
{
    private SlitCoilLabelPrintHistory() { }

    public SlitCoilLabelPrintHistory(Guid slitCoilId, string coilNumber, string labelVersion,
        string? printedBy, DateTimeOffset printedOn, int copies, string? printerName,
        LabelPrintType printType, string? remarks)
    {
        SlitCoilId = slitCoilId;
        CoilNumber = coilNumber;
        LabelVersion = labelVersion;
        PrintedBy = printedBy;
        PrintedOn = printedOn;
        Copies = copies;
        PrinterName = string.IsNullOrWhiteSpace(printerName) ? null : printerName.Trim();
        PrintType = printType;
        Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
    }

    public Guid SlitCoilId { get; private set; }
    public string CoilNumber { get; private set; } = string.Empty;
    public string LabelVersion { get; private set; } = "1";
    public string? PrintedBy { get; private set; }
    public DateTimeOffset PrintedOn { get; private set; }
    public int Copies { get; private set; }
    public string? PrinterName { get; private set; }
    public LabelPrintType PrintType { get; private set; }
    public string? Remarks { get; private set; }
    public SlitCoil? SlitCoil { get; private set; }
}
