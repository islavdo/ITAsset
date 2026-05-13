using ITAssetAccounting.Shared.Enums;
using ITAssetAccounting.Shared.Paging;

namespace ITAssetAccounting.Shared.Dto;

public record CategoryDto(int Id, string Name, string Description);
public record CategoryCreateRequest(string Name, string Description);

public record LocationDto(int Id, string Name, string Address, string Description);
public record LocationCreateRequest(string Name, string Address, string Description);

public record EquipmentDto(
    int Id,
    string InventoryNumber,
    string Name,
    string Model,
    string SerialNumber,
    int CategoryId,
    string CategoryName,
    int LocationId,
    string LocationName,
    EquipmentStatus Status,
    DateTime? PurchaseDate,
    decimal? Price,
    Guid? AssignedUserId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record EquipmentCreateRequest(
    string InventoryNumber,
    string Name,
    string Model,
    string SerialNumber,
    int CategoryId,
    int LocationId,
    DateTime? PurchaseDate,
    decimal? Price);

public record EquipmentUpdateRequest(
    string InventoryNumber,
    string Name,
    string Model,
    string SerialNumber,
    int CategoryId,
    int LocationId,
    DateTime? PurchaseDate,
    decimal? Price,
    EquipmentStatus Status);

public class EquipmentFilter : PagedRequest
{
    public string? Search { get; set; }
    public EquipmentStatus? Status { get; set; }
    public int? CategoryId { get; set; }
    public int? LocationId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? SortBy { get; set; } = "name";
    public bool Desc { get; set; }
}

public record AssignmentDto(
    int Id,
    int EquipmentId,
    Guid UserId,
    DateTime AssignedAtUtc,
    DateTime? ReturnedAtUtc,
    string Comment);

public record AssignEquipmentRequest(Guid UserId, string Comment);
public record ReturnEquipmentRequest(string Comment);

public record EquipmentHistoryDto(
    int Id,
    int EquipmentId,
    string Action,
    EquipmentStatus? OldStatus,
    EquipmentStatus? NewStatus,
    Guid? UserId,
    string Comment,
    DateTime CreatedAtUtc);
