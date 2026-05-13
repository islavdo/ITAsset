namespace ITAssetAccounting.EquipmentService.Entities;

public class Assignment
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public EquipmentItem Equipment { get; set; } = null!;
    public Guid UserId { get; set; }
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnedAtUtc { get; set; }
    public string Comment { get; set; } = string.Empty;
}
