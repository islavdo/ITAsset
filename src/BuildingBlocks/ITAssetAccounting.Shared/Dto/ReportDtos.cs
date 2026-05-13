using ITAssetAccounting.Shared.Enums;

namespace ITAssetAccounting.Shared.Dto;

public record DashboardDto(
    int TotalEquipment,
    int InStock,
    int Assigned,
    int InRepair,
    int WrittenOff,
    IReadOnlyCollection<StatusCountDto> ByStatus,
    IReadOnlyCollection<CategoryCountDto> ByCategory,
    IReadOnlyCollection<LocationCountDto> ByLocation,
    DateTime GeneratedAtUtc);

public record StatusCountDto(EquipmentStatus Status, int Count);
public record CategoryCountDto(int CategoryId, string CategoryName, int Count);
public record LocationCountDto(int LocationId, string LocationName, int Count);
