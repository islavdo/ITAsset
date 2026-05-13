using ITAssetAccounting.FileService.Data;
using ITAssetAccounting.FileService.Entities;
using ITAssetAccounting.FileService.Mapping;
using ITAssetAccounting.FileService.Services;
using ITAssetAccounting.Infrastructure.Auth;
using ITAssetAccounting.Shared.Api;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.FileService.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly FileDbContext _db;
    private readonly IFileStorage _storage;

    public FilesController(FileDbContext db, IFileStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet("equipment/{equipmentId:int}")]
    public async Task<ActionResult<IReadOnlyCollection<EquipmentFileDto>>> GetByEquipment(int equipmentId, CancellationToken ct)
    {
        var files = await _db.EquipmentFiles
            .Where(f => f.EquipmentId == equipmentId)
            .OrderByDescending(f => f.UploadedAtUtc)
            .ToListAsync(ct);
        return Ok(files.Select(f => f.ToDto()).ToArray());
    }

    [HttpPost("equipment/{equipmentId:int}")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(int equipmentId, IFormFile file, [FromForm] FileKind kind, CancellationToken ct)
    {
        try
        {
            var saved = await _storage.SaveAsync(file, ct);
            var entity = new EquipmentFile
            {
                EquipmentId = equipmentId,
                OriginalFileName = Path.GetFileName(file.FileName),
                StoredFileName = saved.storedFileName,
                ContentType = file.ContentType,
                Size = saved.size,
                Kind = kind,
                UploadedByUserId = User.GetUserId()
            };
            await _db.EquipmentFiles.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(Download), new { id = entity.Id }, entity.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiError("file_validation_failed", ex.Message));
        }
    }

    [HttpGet("download/{id:int}")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var file = await _db.EquipmentFiles.FindAsync(new object?[] { id }, ct);
        if (file is null) return NotFound(new ApiError("not_found", "File not found."));
        var stream = await _storage.OpenReadAsync(file.StoredFileName, ct);
        return File(stream, file.ContentType, file.OriginalFileName);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var file = await _db.EquipmentFiles.FindAsync(new object?[] { id }, ct);
        if (file is null) return NotFound(new ApiError("not_found", "File not found."));
        await _storage.DeleteAsync(file.StoredFileName, ct);
        _db.EquipmentFiles.Remove(file);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
