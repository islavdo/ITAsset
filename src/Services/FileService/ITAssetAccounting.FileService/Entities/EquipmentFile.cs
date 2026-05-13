using ITAssetAccounting.Shared.Enums;

namespace ITAssetAccounting.FileService.Entities;

public class EquipmentFile
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public FileKind Kind { get; set; } = FileKind.Other;
    public Guid UploadedByUserId { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
