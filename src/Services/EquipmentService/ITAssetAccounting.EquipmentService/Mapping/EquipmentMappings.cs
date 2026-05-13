using ITAssetAccounting.EquipmentService.Entities;
using ITAssetAccounting.Shared.Dto;

namespace ITAssetAccounting.EquipmentService.Mapping;

public static class EquipmentMappings
{
    public static CategoryDto ToDto(this Category c) => new(c.Id, c.Name, c.Description);
    public static LocationDto ToDto(this Location l) => new(l.Id, l.Name, l.Address, l.Description);

    public static EquipmentDto ToDto(this EquipmentItem e) => new(
        e.Id,
        e.InventoryNumber,
        e.Name,
        e.Model,
        e.SerialNumber,
        e.CategoryId,
        e.Category?.Name ?? string.Empty,
        e.LocationId,
        e.Location?.Name ?? string.Empty,
        e.Status,
        e.PurchaseDate,
        e.Price,
        e.AssignedUserId,
        e.CreatedAtUtc,
        e.UpdatedAtUtc);

    public static AssignmentDto ToDto(this Assignment a) => new(a.Id, a.EquipmentId, a.UserId, a.AssignedAtUtc, a.ReturnedAtUtc, a.Comment);

    public static EquipmentHistoryDto ToDto(this EquipmentHistory h) => new(h.Id, h.EquipmentId, h.Action, h.OldStatus, h.NewStatus, h.UserId, h.Comment, h.CreatedAtUtc);
}
