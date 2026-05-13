using ITAssetAccounting.EquipmentService.Data;
using ITAssetAccounting.EquipmentService.Entities;
using ITAssetAccounting.EquipmentService.Hubs;
using ITAssetAccounting.EquipmentService.Mapping;
using ITAssetAccounting.EquipmentService.Repositories;
using ITAssetAccounting.Infrastructure.Auth;
using ITAssetAccounting.Shared.Api;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Enums;
using ITAssetAccounting.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.EquipmentService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EquipmentController : ControllerBase
{
    private readonly EquipmentUnitOfWork _uow;
    private readonly EquipmentDbContext _db;
    private readonly IHubContext<EquipmentHub> _hub;

    public EquipmentController(EquipmentUnitOfWork uow, EquipmentDbContext db, IHubContext<EquipmentHub> hub)
    {
        _uow = uow;
        _db = db;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EquipmentDto>>> GetPaged([FromQuery] EquipmentFilter filter, CancellationToken ct)
    {
        var page = await _uow.Equipments.GetPagedAsync(filter, ct);
        return Ok(new PagedResult<EquipmentDto>
        {
            Items = page.Items.Select(x => x.ToDto()).ToArray(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalItems = page.TotalItems
        });
    }

    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyCollection<EquipmentDto>>> GetAllForReports(CancellationToken ct)
    {
        var items = await _db.Equipments.Include(e => e.Category).Include(e => e.Location).AsNoTracking().ToListAsync(ct);
        return Ok(items.Select(e => e.ToDto()).ToArray());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EquipmentDto>> GetById(int id, CancellationToken ct)
    {
        var item = await _uow.Equipments.GetByIdAsync(id, ct);
        return item is null ? NotFound(new ApiError("not_found", "Equipment not found.")) : Ok(item.ToDto());
    }

    [HttpPost]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Create(EquipmentCreateRequest request, CancellationToken ct)
    {
        if (await _uow.Equipments.GetByInventoryNumberAsync(request.InventoryNumber.Trim(), ct) is not null)
        {
            return Conflict(new ApiError("inventory_exists", "Inventory number must be unique."));
        }

        var item = new EquipmentItem
        {
            InventoryNumber = request.InventoryNumber.Trim(),
            Name = request.Name.Trim(),
            Model = request.Model.Trim(),
            SerialNumber = request.SerialNumber.Trim(),
            CategoryId = request.CategoryId,
            LocationId = request.LocationId,
            PurchaseDate = request.PurchaseDate,
            Price = request.Price,
            Status = EquipmentStatus.InStock
        };
        item.History.Add(new EquipmentHistory
        {
            Action = "Created",
            NewStatus = EquipmentStatus.InStock,
            UserId = User.GetUserId(),
            Comment = "Equipment created"
        });
        await _uow.Equipments.AddAsync(item, ct);
        await _uow.SaveChangesAsync(ct);
        await _hub.Clients.All.SendAsync("equipmentCreated", item.ToDto(), ct);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Update(int id, EquipmentUpdateRequest request, CancellationToken ct)
    {
        var item = await _uow.Equipments.GetByIdAsync(id, ct);
        if (item is null) return NotFound(new ApiError("not_found", "Equipment not found."));
        var oldStatus = item.Status;
        item.InventoryNumber = request.InventoryNumber.Trim();
        item.Name = request.Name.Trim();
        item.Model = request.Model.Trim();
        item.SerialNumber = request.SerialNumber.Trim();
        item.CategoryId = request.CategoryId;
        item.LocationId = request.LocationId;
        item.PurchaseDate = request.PurchaseDate;
        item.Price = request.Price;
        item.Status = request.Status;
        item.UpdatedAtUtc = DateTime.UtcNow;
        item.History.Add(new EquipmentHistory
        {
            Action = "Updated",
            OldStatus = oldStatus,
            NewStatus = request.Status,
            UserId = User.GetUserId(),
            Comment = "Equipment updated"
        });
        _uow.Equipments.Update(item);
        await _uow.SaveChangesAsync(ct);
        await _hub.Clients.Group($"equipment:{id}").SendAsync("equipmentUpdated", item.ToDto(), ct);
        await _hub.Clients.All.SendAsync("equipmentUpdated", item.ToDto(), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _uow.Equipments.GetByIdAsync(id, ct);
        if (item is null) return NotFound(new ApiError("not_found", "Equipment not found."));
        var oldStatus = item.Status;
        item.Status = EquipmentStatus.WrittenOff;
        item.UpdatedAtUtc = DateTime.UtcNow;
        item.History.Add(new EquipmentHistory
        {
            Action = "WrittenOff",
            OldStatus = oldStatus,
            NewStatus = EquipmentStatus.WrittenOff,
            UserId = User.GetUserId(),
            Comment = "Soft delete / write-off"
        });
        await _uow.SaveChangesAsync(ct);
        await _hub.Clients.All.SendAsync("equipmentWrittenOff", id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/assign")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Assign(int id, AssignEquipmentRequest request, CancellationToken ct)
    {
        var item = await _uow.Equipments.GetByIdAsync(id, ct);
        if (item is null) return NotFound(new ApiError("not_found", "Equipment not found."));
        if (item.Status == EquipmentStatus.WrittenOff) return BadRequest(new ApiError("invalid_status", "Written off equipment cannot be assigned."));

        var oldStatus = item.Status;
        item.AssignedUserId = request.UserId;
        item.Status = EquipmentStatus.Assigned;
        item.UpdatedAtUtc = DateTime.UtcNow;
        item.Assignments.Add(new Assignment { EquipmentId = id, UserId = request.UserId, Comment = request.Comment.Trim() });
        item.History.Add(new EquipmentHistory
        {
            Action = "Assigned",
            OldStatus = oldStatus,
            NewStatus = EquipmentStatus.Assigned,
            UserId = request.UserId,
            Comment = request.Comment.Trim()
        });
        await _uow.SaveChangesAsync(ct);
        await _hub.Clients.All.SendAsync("equipmentAssigned", item.ToDto(), ct);
        return Ok(item.ToDto());
    }

    [HttpPut("{id:int}/return")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Return(int id, ReturnEquipmentRequest request, CancellationToken ct)
    {
        var item = await _uow.Equipments.GetByIdAsync(id, ct);
        if (item is null) return NotFound(new ApiError("not_found", "Equipment not found."));
        var assignment = item.Assignments.FirstOrDefault(a => a.ReturnedAtUtc is null);
        if (assignment is not null) assignment.ReturnedAtUtc = DateTime.UtcNow;
        var oldStatus = item.Status;
        item.AssignedUserId = null;
        item.Status = EquipmentStatus.InStock;
        item.UpdatedAtUtc = DateTime.UtcNow;
        item.History.Add(new EquipmentHistory
        {
            Action = "Returned",
            OldStatus = oldStatus,
            NewStatus = EquipmentStatus.InStock,
            UserId = User.GetUserId(),
            Comment = request.Comment.Trim()
        });
        await _uow.SaveChangesAsync(ct);
        await _hub.Clients.All.SendAsync("equipmentReturned", item.ToDto(), ct);
        return Ok(item.ToDto());
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<IReadOnlyCollection<EquipmentHistoryDto>>> History(int id, CancellationToken ct)
    {
        var history = await _db.EquipmentHistory
            .Where(h => h.EquipmentId == id)
            .OrderByDescending(h => h.CreatedAtUtc)
            .ToListAsync(ct);
        return Ok(history.Select(h => h.ToDto()).ToArray());
    }
}
