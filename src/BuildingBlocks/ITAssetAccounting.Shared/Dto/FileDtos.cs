using ITAssetAccounting.Shared.Enums;

namespace ITAssetAccounting.Shared.Dto;

public record EquipmentFileDto(
    int Id,
    int EquipmentId,
    string OriginalFileName,
    string ContentType,
    long Size,
    FileKind Kind,
    Guid UploadedByUserId,
    DateTime UploadedAtUtc);
