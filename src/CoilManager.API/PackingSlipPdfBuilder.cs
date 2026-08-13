using System.Text;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;

namespace CoilManager.API;

public static class PackingSlipPdfBuilder
{
    public static byte[] Create(Dispatch dispatch)
    {
        var lines = new List<string>
        {
            dispatch.Status == DispatchStatus.Draft ? "DRAFT - PACKING SLIP" : "PACKING SLIP",
            dispatch.PackingSlipNumber,
            $"Dispatch: {dispatch.DispatchNumber}",
            $"Customer: {dispatch.CustomerName}",
            $"Work Order: {dispatch.WorkOrderNumber}",
            $"Quantity: {dispatch.DispatchQuantity:N3} {dispatch.QuantityUnit}",
            $"Ship To: {dispatch.ShippingAddress}"
        };
        lines.AddRange(dispatch.InventorySources.Select(x => $"{x.InventoryNumber}  {x.Width:N3} mm  {x.Quantity:N3} kg"));
        lines.AddRange(dispatch.Packages.Select(x => $"{x.PackageNumber}  {x.Description}  {x.Quantity:N3} {x.QuantityUnit}"));
        string text = string.Join(") Tj T* (", lines.Select(Safe));
        string stream = $"BT /F1 11 Tf 50 790 Td 14 TL ({text}) Tj ET";
        const string nl = "\n";
        string[] objects =
        [
            "1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj",
            "2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj",
            "3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>endobj",
            "4 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj",
            $"5 0 obj<< /Length {Encoding.ASCII.GetByteCount(stream)} >>stream{nl}{stream}{nl}endstream endobj"
        ];
        using var output = new MemoryStream();
        void Write(string value) => output.Write(Encoding.ASCII.GetBytes(value));
        Write("%PDF-1.4" + nl); var offsets = new List<long> { 0 };
        foreach (string item in objects) { offsets.Add(output.Position); Write(item + nl); }
        long xref = output.Position; Write($"xref{nl}0 {objects.Length + 1}{nl}0000000000 65535 f {nl}");
        for (int i = 1; i < offsets.Count; i++) Write($"{offsets[i]:0000000000} 00000 n {nl}");
        Write($"trailer<< /Size {objects.Length + 1} /Root 1 0 R >>{nl}startxref{nl}{xref}{nl}%%EOF");
        return output.ToArray();
    }

    private static string Safe(string value) => value.Replace("(", "[").Replace(")", "]");
}
