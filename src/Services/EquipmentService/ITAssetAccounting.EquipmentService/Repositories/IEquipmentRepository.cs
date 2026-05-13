using ITAssetAccounting.EquipmentService.Entities;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Paging;

namespace ITAssetAccounting.EquipmentService.Repositories;

public interface IEquipmentRepository
{
    Task<PagedResult<EquipmentItem>> GetPagedAsync(EquipmentFilter filter, CancellationToken ct = default);
    Task<EquipmentItem?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<EquipmentItem?> GetByInventoryNumberAsync(string inventoryNumber, CancellationToken ct = default);
    Task AddAsync(EquipmentItem item, CancellationToken ct = default);
    void Update(EquipmentItem item);
    void Delete(EquipmentItem item);
}
