using ITAssetAccounting.Shared.Enums;

namespace ITAssetAccounting.EquipmentService.Entities;

public class EquipmentHistory
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public EquipmentItem Equipment { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public EquipmentStatus? OldStatus { get; set; }
    public EquipmentStatus? NewStatus { get; set; }
    public Guid? UserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
