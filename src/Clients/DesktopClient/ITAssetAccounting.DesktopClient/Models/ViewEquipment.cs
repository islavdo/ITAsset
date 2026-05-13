using ITAssetAccounting.Shared.Enums;

namespace ITAssetAccounting.DesktopClient.Models;

public record ViewEquipment(int Id, string InventoryNumber, string Name, string Model, string LocationName, EquipmentStatus Status);
