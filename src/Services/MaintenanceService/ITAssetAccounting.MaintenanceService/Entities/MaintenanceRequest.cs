using ITAssetAccounting.Shared.Enums;

namespace ITAssetAccounting.MaintenanceService.Entities;

public class MaintenanceRequest
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.New;
    public MaintenancePriority Priority { get; set; } = MaintenancePriority.Normal;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public ICollection<MaintenanceComment> Comments { get; set; } = new List<MaintenanceComment>();
}
