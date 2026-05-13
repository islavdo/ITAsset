using ITAssetAccounting.EquipmentService.Data;
using ITAssetAccounting.EquipmentService.Entities;
using ITAssetAccounting.Infrastructure.Paging;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Paging;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.EquipmentService.Repositories;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly EquipmentDbContext _db;

    public EquipmentRepository(EquipmentDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<EquipmentItem>> GetPagedAsync(EquipmentFilter filter, CancellationToken ct = default)
    {
        var query = _db.Equipments.Include(e => e.Category).Include(e => e.Location).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(e => e.InventoryNumber.ToLower().Contains(search)
                || e.Name.ToLower().Contains(search)
                || e.Model.ToLower().Contains(search)
                || e.SerialNumber.ToLower().Contains(search));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(e => e.Status == filter.Status.Value);
        }
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == filter.CategoryId.Value);
        }
        if (filter.LocationId.HasValue)
        {
            query = query.Where(e => e.LocationId == filter.LocationId.Value);
        }
        if (filter.AssignedUserId.HasValue)
        {
            query = query.Where(e => e.AssignedUserId == filter.AssignedUserId.Value);
        }

        query = (filter.SortBy?.ToLowerInvariant(), filter.Desc) switch
        {
            ("inventory", true) => query.OrderByDescending(e => e.InventoryNumber),
            ("inventory", false) => query.OrderBy(e => e.InventoryNumber),
            ("status", true) => query.OrderByDescending(e => e.Status),
            ("status", false) => query.OrderBy(e => e.Status),
            ("created", true) => query.OrderByDescending(e => e.CreatedAtUtc),
            ("created", false) => query.OrderBy(e => e.CreatedAtUtc),
            (_, true) => query.OrderByDescending(e => e.Name),
            _ => query.OrderBy(e => e.Name)
        };

        return await query.ToPagedResultAsync(filter, ct);
    }

    public Task<EquipmentItem?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Equipments.Include(e => e.Category).Include(e => e.Location)
            .Include(e => e.Assignments).Include(e => e.History)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<EquipmentItem?> GetByInventoryNumberAsync(string inventoryNumber, CancellationToken ct = default) =>
        _db.Equipments.FirstOrDefaultAsync(e => e.InventoryNumber == inventoryNumber, ct);

    public Task AddAsync(EquipmentItem item, CancellationToken ct = default) => _db.Equipments.AddAsync(item, ct).AsTask();
    public void Update(EquipmentItem item) => _db.Equipments.Update(item);
    public void Delete(EquipmentItem item) => _db.Equipments.Remove(item);
}
