using ITAssetAccounting.MaintenanceService.Entities;
using ITAssetAccounting.Shared.Dto;

namespace ITAssetAccounting.MaintenanceService.Mapping;

public static class MaintenanceMappings
{
    public static MaintenanceCommentDto ToDto(this MaintenanceComment c) =>
        new(c.Id, c.MaintenanceRequestId, c.AuthorUserId, c.Text, c.CreatedAtUtc);

    public static MaintenanceRequestDto ToDto(this MaintenanceRequest r) =>
        new(r.Id, r.EquipmentId, r.CreatedByUserId, r.AssignedToUserId, r.Title, r.Description, r.Status, r.Priority, r.CreatedAtUtc, r.CompletedAtUtc, r.Comments.Select(c => c.ToDto()).ToArray());
}
