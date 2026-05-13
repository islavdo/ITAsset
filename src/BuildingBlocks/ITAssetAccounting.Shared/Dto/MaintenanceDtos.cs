using ITAssetAccounting.Shared.Enums;
using ITAssetAccounting.Shared.Paging;

namespace ITAssetAccounting.Shared.Dto;

public record MaintenanceRequestDto(
    int Id,
    int EquipmentId,
    Guid CreatedByUserId,
    Guid? AssignedToUserId,
    string Title,
    string Description,
    MaintenanceStatus Status,
    MaintenancePriority Priority,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    IReadOnlyCollection<MaintenanceCommentDto> Comments);

public record MaintenanceCreateRequest(
    int EquipmentId,
    string Title,
    string Description,
    MaintenancePriority Priority);

public record MaintenanceUpdateRequest(
    string Title,
    string Description,
    MaintenancePriority Priority);

public record MaintenanceStatusUpdateRequest(MaintenanceStatus Status, string Comment);
public record MaintenanceAssignRequest(Guid ItSpecialistId);
public record MaintenanceCommentCreateRequest(string Text);

public record MaintenanceCommentDto(
    int Id,
    int MaintenanceRequestId,
    Guid AuthorUserId,
    string Text,
    DateTime CreatedAtUtc);

public class MaintenanceFilter : PagedRequest
{
    public int? EquipmentId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public MaintenanceStatus? Status { get; set; }
    public MaintenancePriority? Priority { get; set; }
    public string? Search { get; set; }
}
