namespace ITAssetAccounting.EquipmentService.Entities;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<EquipmentItem> Equipments { get; set; } = new List<EquipmentItem>();
}
