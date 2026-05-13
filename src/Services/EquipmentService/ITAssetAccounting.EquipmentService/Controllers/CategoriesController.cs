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
public class CategoriesController : ControllerBase
{
    private readonly EquipmentDbContext _db;
    private readonly ICacheService _cache;
    private const string CacheKey = "equipment:categories";

    public CategoriesController(EquipmentDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var cached = await _cache.GetAsync<IReadOnlyCollection<CategoryDto>>(CacheKey, ct);
        if (cached is not null) return Ok(cached);
        var entities = await _db.Categories.OrderBy(c => c.Name).ToListAsync(ct);
        var result = entities.Select(c => c.ToDto()).ToArray();
        await _cache.SetAsync(CacheKey, result, TimeSpan.FromMinutes(10), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Create(CategoryCreateRequest request, CancellationToken ct)
    {
        var category = new Category { Name = request.Name.Trim(), Description = request.Description.Trim() };
        await _db.Categories.AddAsync(category, ct);
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKey, ct);
        return CreatedAtAction(nameof(GetAll), new { id = category.Id }, category.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Update(int id, CategoryCreateRequest request, CancellationToken ct)
    {
        var category = await _db.Categories.FindAsync(new object?[] { id }, ct);
        if (category is null) return NotFound(new ApiError("not_found", "Category not found."));
        category.Name = request.Name.Trim();
        category.Description = request.Description.Trim();
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKey, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var category = await _db.Categories.FindAsync(new object?[] { id }, ct);
        if (category is null) return NotFound(new ApiError("not_found", "Category not found."));
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKey, ct);
        return NoContent();
    }
}
