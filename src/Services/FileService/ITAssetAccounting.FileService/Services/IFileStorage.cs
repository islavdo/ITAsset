namespace ITAssetAccounting.FileService.Services;

public interface IFileStorage
{
    Task<(string storedFileName, long size)> SaveAsync(IFormFile file, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string storedFileName, CancellationToken ct = default);
    Task DeleteAsync(string storedFileName, CancellationToken ct = default);
    void Validate(IFormFile file);
}
