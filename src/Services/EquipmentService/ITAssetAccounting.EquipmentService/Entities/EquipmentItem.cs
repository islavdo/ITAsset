using ITAssetAccounting.Shared.Enums;

namespace ITAssetAccounting.EquipmentService.Entities;

public class EquipmentItem
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public EquipmentStatus Status { get; set; } = EquipmentStatus.InStock;
    public DateTime? PurchaseDate { get; set; }
    public decimal? Price { get; set; }
    public Guid? AssignedUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<EquipmentHistory> History { get; set; } = new List<EquipmentHistory>();
}
