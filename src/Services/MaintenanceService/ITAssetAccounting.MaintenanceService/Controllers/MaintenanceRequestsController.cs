using ITAssetAccounting.Infrastructure.Auth;
using ITAssetAccounting.Infrastructure.Paging;
using ITAssetAccounting.MaintenanceService.Data;
using ITAssetAccounting.MaintenanceService.Entities;
using ITAssetAccounting.MaintenanceService.Hubs;
using ITAssetAccounting.MaintenanceService.Mapping;
using ITAssetAccounting.Shared.Api;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Enums;
using ITAssetAccounting.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.MaintenanceService.Controllers;

[ApiController]
[Route("api/maintenance")]
[Authorize]
public class MaintenanceRequestsController : ControllerBase
{
    private readonly MaintenanceDbContext _db;
    private readonly IHubContext<MaintenanceHub> _hub;

    public MaintenanceRequestsController(MaintenanceDbContext db, IHubContext<MaintenanceHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<MaintenanceRequestDto>>> GetPaged([FromQuery] MaintenanceFilter filter, CancellationToken ct)
    {
        var query = _db.MaintenanceRequests.Include(r => r.Comments).AsQueryable();

        if (filter.EquipmentId.HasValue) query = query.Where(r => r.EquipmentId == filter.EquipmentId.Value);
        if (filter.CreatedByUserId.HasValue) query = query.Where(r => r.CreatedByUserId == filter.CreatedByUserId.Value);
        if (filter.AssignedToUserId.HasValue) query = query.Where(r => r.AssignedToUserId == filter.AssignedToUserId.Value);
        if (filter.Status.HasValue) query = query.Where(r => r.Status == filter.Status.Value);
        if (filter.Priority.HasValue) query = query.Where(r => r.Priority == filter.Priority.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(s) || r.Description.ToLower().Contains(s));
        }

        query = query.OrderByDescending(r => r.CreatedAtUtc);
        var entityPage = await query.ToPagedResultAsync(filter, ct);
        return Ok(new PagedResult<MaintenanceRequestDto>
        {
            Items = entityPage.Items.Select(r => r.ToDto()).ToArray(),
            Page = entityPage.Page,
            PageSize = entityPage.PageSize,
            TotalItems = entityPage.TotalItems
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MaintenanceRequestDto>> GetById(int id, CancellationToken ct)
    {
        var req = await _db.MaintenanceRequests.Include(r => r.Comments).FirstOrDefaultAsync(r => r.Id == id, ct);
        return req is null ? NotFound(new ApiError("not_found", "Maintenance request not found.")) : Ok(req.ToDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create(MaintenanceCreateRequest request, CancellationToken ct)
    {
        var entity = new MaintenanceRequest
        {
            EquipmentId = request.EquipmentId,
            CreatedByUserId = User.GetUserId(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            Status = MaintenanceStatus.New
        };
        await _db.MaintenanceRequests.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        await _hub.Clients.All.SendAsync("maintenanceCreated", entity.ToDto(), ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, MaintenanceUpdateRequest request, CancellationToken ct)
    {
        var entity = await _db.MaintenanceRequests.Include(r => r.Comments).FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return NotFound(new ApiError("not_found", "Maintenance request not found."));
        if (entity.CreatedByUserId != User.GetUserId() && !User.IsInRole("Admin") && !User.IsInRole("ItSpecialist"))
        {
            return Forbid();
        }
        entity.Title = request.Title.Trim();
        entity.Description = request.Description.Trim();
        entity.Priority = request.Priority;
        await _db.SaveChangesAsync(ct);
        await _hub.Clients.Group($"maintenance:{id}").SendAsync("maintenanceUpdated", entity.ToDto(), ct);
        await _hub.Clients.All.SendAsync("maintenanceUpdated", entity.ToDto(), ct);
        return NoContent();
    }

    [HttpPut("{id:int}/assign")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Assign(int id, MaintenanceAssignRequest request, CancellationToken ct)
    {
        var entity = await _db.MaintenanceRequests.Include(r => r.Comments).FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return NotFound(new ApiError("not_found", "Maintenance request not found."));
        entity.AssignedToUserId = request.ItSpecialistId;
        entity.Status = MaintenanceStatus.InProgress;
        await _db.SaveChangesAsync(ct);
        await _hub.Clients.All.SendAsync("maintenanceAssigned", entity.ToDto(), ct);
        return Ok(entity.ToDto());
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> UpdateStatus(int id, MaintenanceStatusUpdateRequest request, CancellationToken ct)
    {
        var entity = await _db.MaintenanceRequests.Include(r => r.Comments).FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return NotFound(new ApiError("not_found", "Maintenance request not found."));
        entity.Status = request.Status;
        if (request.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            entity.CompletedAtUtc = DateTime.UtcNow;
        }
        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            entity.Comments.Add(new MaintenanceComment { AuthorUserId = User.GetUserId(), Text = request.Comment.Trim() });
        }
        await _db.SaveChangesAsync(ct);
        await _hub.Clients.All.SendAsync("maintenanceStatusChanged", entity.ToDto(), ct);
        return Ok(entity.ToDto());
    }

    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, MaintenanceCommentCreateRequest request, CancellationToken ct)
    {
        var entity = await _db.MaintenanceRequests.Include(r => r.Comments).FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return NotFound(new ApiError("not_found", "Maintenance request not found."));
        var comment = new MaintenanceComment { MaintenanceRequestId = id, AuthorUserId = User.GetUserId(), Text = request.Text.Trim() };
        entity.Comments.Add(comment);
        await _db.SaveChangesAsync(ct);
        await _hub.Clients.Group($"maintenance:{id}").SendAsync("maintenanceCommentAdded", comment.ToDto(), ct);
        return Ok(comment.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ItOrAdmin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await _db.MaintenanceRequests.FindAsync(new object?[] { id }, ct);
        if (entity is null) return NotFound(new ApiError("not_found", "Maintenance request not found."));
        _db.MaintenanceRequests.Remove(entity);
        await _db.SaveChangesAsync(ct);
        await _hub.Clients.All.SendAsync("maintenanceDeleted", id, ct);
        return NoContent();
    }
}
