using ITAssetAccounting.EquipmentService.Data;
using ITAssetAccounting.EquipmentService.Entities;
using ITAssetAccounting.EquipmentService.Mapping;
using ITAssetAccounting.Infrastructure.Caching;
using ITAssetAccounting.Shared.Api;
using ITAssetAccounting.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.EquipmentService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly EquipmentDbContext _db;
    private readonly ICacheService _cache;
    private const string CacheKey = "equipment:locations";

    public LocationsController(EquipmentDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LocationDto>>> GetAll(CancellationToken ct)
    {
        var cached = await _cache.GetAsync<IReadOnlyCollection<LocationDto>>(CacheKey, ct);
        if (cached is not null) return Ok(cached);
        var entities = await _db.Locations.OrderBy(l => l.Name).ToListAsync(ct);
        var result = entities.Select(l => l.ToDto()).ToArray();
        await _cache.SetAsync(CacheKey, result, TimeSpan.FromMinutes(10), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Create(LocationCreateRequest request, CancellationToken ct)
    {
        var location = new Location { Name = request.Name.Trim(), Address = request.Address.Trim(), Description = request.Description.Trim() };
        await _db.Locations.AddAsync(location, ct);
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKey, ct);
        return CreatedAtAction(nameof(GetAll), new { id = location.Id }, location.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Update(int id, LocationCreateRequest request, CancellationToken ct)
    {
        var location = await _db.Locations.FindAsync(new object?[] { id }, ct);
        if (location is null) return NotFound(new ApiError("not_found", "Location not found."));
        location.Name = request.Name.Trim();
        location.Address = request.Address.Trim();
        location.Description = request.Description.Trim();
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKey, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var location = await _db.Locations.FindAsync(new object?[] { id }, ct);
        if (location is null) return NotFound(new ApiError("not_found", "Location not found."));
        _db.Locations.Remove(location);
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKey, ct);
        return NoContent();
    }
}
