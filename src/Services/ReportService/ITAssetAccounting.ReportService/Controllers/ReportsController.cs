using ITAssetAccounting.Infrastructure.Caching;
using ITAssetAccounting.ReportService.Services;
using ITAssetAccounting.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetAccounting.ReportService.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly EquipmentApiClient _equipmentClient;
    private readonly ReportGenerator _generator;
    private readonly ICacheService _cache;

    public ReportsController(EquipmentApiClient equipmentClient, ReportGenerator generator, ICacheService cache)
    {
        _equipmentClient = equipmentClient;
        _generator = generator;
        _cache = cache;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard(CancellationToken ct)
    {
        const string cacheKey = "reports:dashboard";
        var cached = await _cache.GetAsync<DashboardDto>(cacheKey, ct);
        if (cached is not null) return Ok(cached);
        var equipment = await _equipmentClient.GetAllEquipmentAsync(ct);
        var dashboard = _generator.BuildDashboard(equipment);
        await _cache.SetAsync(cacheKey, dashboard, TimeSpan.FromMinutes(2), ct);
        return Ok(dashboard);
    }

    [HttpGet("equipment/excel")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> ExportExcel(CancellationToken ct)
    {
        var equipment = await _equipmentClient.GetAllEquipmentAsync(ct);
        var bytes = _generator.BuildExcel(equipment);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"equipment-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("equipment/pdf")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> ExportPdf(CancellationToken ct)
    {
        var equipment = await _equipmentClient.GetAllEquipmentAsync(ct);
        var bytes = _generator.BuildPdf(equipment);
        return File(bytes, "application/pdf", $"equipment-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
