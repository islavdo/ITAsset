namespace ITAssetAccounting.FileService.Services;

public class FileStorageOptions
{
    public string RootPath { get; set; } = "storage";
    public long MaxBytes { get; set; } = 10 * 1024 * 1024;
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
}
