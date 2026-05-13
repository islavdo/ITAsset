using ITAssetAccounting.FileService.Entities;
using ITAssetAccounting.Shared.Dto;

namespace ITAssetAccounting.FileService.Mapping;

public static class FileMappings
{
    public static EquipmentFileDto ToDto(this EquipmentFile file) =>
        new(file.Id, file.EquipmentId, file.OriginalFileName, file.ContentType, file.Size, file.Kind, file.UploadedByUserId, file.UploadedAtUtc);
}
