using ClosedXML.Excel;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Enums;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace ITAssetAccounting.ReportService.Services;

public class ReportGenerator
{
    public DashboardDto BuildDashboard(IReadOnlyCollection<EquipmentDto> equipment)
    {
        return new DashboardDto(
            equipment.Count,
            equipment.Count(e => e.Status == EquipmentStatus.InStock),
            equipment.Count(e => e.Status == EquipmentStatus.Assigned),
            equipment.Count(e => e.Status == EquipmentStatus.InRepair),
            equipment.Count(e => e.Status == EquipmentStatus.WrittenOff),
            equipment.GroupBy(e => e.Status).Select(g => new StatusCountDto(g.Key, g.Count())).ToArray(),
            equipment.GroupBy(e => new { e.CategoryId, e.CategoryName }).Select(g => new CategoryCountDto(g.Key.CategoryId, g.Key.CategoryName, g.Count())).ToArray(),
            equipment.GroupBy(e => new { e.LocationId, e.LocationName }).Select(g => new LocationCountDto(g.Key.LocationId, g.Key.LocationName, g.Count())).ToArray(),
            DateTime.UtcNow);
    }

    public byte[] BuildExcel(IReadOnlyCollection<EquipmentDto> equipment)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Equipment");
        var headers = new[] { "ID", "Inventory", "Name", "Model", "Serial", "Category", "Location", "Status", "AssignedUser", "Price" };
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
        var row = 2;
        foreach (var item in equipment)
        {
            sheet.Cell(row, 1).Value = item.Id;
            sheet.Cell(row, 2).Value = item.InventoryNumber;
            sheet.Cell(row, 3).Value = item.Name;
            sheet.Cell(row, 4).Value = item.Model;
            sheet.Cell(row, 5).Value = item.SerialNumber;
            sheet.Cell(row, 6).Value = item.CategoryName;
            sheet.Cell(row, 7).Value = item.LocationName;
            sheet.Cell(row, 8).Value = item.Status.ToString();
            sheet.Cell(row, 9).Value = item.AssignedUserId?.ToString() ?? "";
            sheet.Cell(row, 10).Value = item.Price ?? 0;
            row++;
        }
        sheet.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] BuildPdf(IReadOnlyCollection<EquipmentDto> equipment)
    {
        var doc = new PdfDocument();
        doc.Info.Title = "Equipment report";
        var page = doc.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
        var font = new XFont("Arial", 10, XFontStyle.Regular);
        gfx.DrawString("IT Asset Accounting - Equipment Report", titleFont, XBrushes.Black, new XPoint(40, 50));
        gfx.DrawString($"Generated: {DateTime.UtcNow:u}", font, XBrushes.Black, new XPoint(40, 75));
        var y = 110;
        foreach (var item in equipment.Take(35))
        {
            gfx.DrawString($"{item.InventoryNumber} | {item.Name} | {item.Status} | {item.LocationName}", font, XBrushes.Black, new XPoint(40, y));
            y += 18;
        }
        if (equipment.Count > 35)
        {
            gfx.DrawString($"... and {equipment.Count - 35} more items", font, XBrushes.Black, new XPoint(40, y + 10));
        }
        using var ms = new MemoryStream();
        doc.Save(ms, false);
        return ms.ToArray();
    }
}
