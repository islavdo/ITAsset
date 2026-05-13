using Microsoft.Extensions.Options;

namespace ITAssetAccounting.FileService.Services;

public class LocalFileStorage : IFileStorage
{
    private readonly FileStorageOptions _options;

    public LocalFileStorage(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;
        Directory.CreateDirectory(_options.RootPath);
    }

    public void Validate(IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("Empty file is not allowed.");
        }
        if (file.Length > _options.MaxBytes)
        {
            throw new InvalidOperationException($"File size exceeds {_options.MaxBytes} bytes.");
        }
        if (_options.AllowedContentTypes.Length > 0 && !_options.AllowedContentTypes.Contains(file.ContentType))
        {
            throw new InvalidOperationException($"Content type {file.ContentType} is not allowed.");
        }
    }

    public async Task<(string storedFileName, long size)> SaveAsync(IFormFile file, CancellationToken ct = default)
    {
        Validate(file);
        var extension = Path.GetExtension(file.FileName);
        var stored = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(_options.RootPath, stored);
        await using var fs = File.Create(path);
        await file.CopyToAsync(fs, ct);
        return (stored, file.Length);
    }

    public Task<Stream> OpenReadAsync(string storedFileName, CancellationToken ct = default)
    {
        var path = Path.Combine(_options.RootPath, storedFileName);
        Stream stream = File.OpenRead(path);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storedFileName, CancellationToken ct = default)
    {
        var path = Path.Combine(_options.RootPath, storedFileName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
}
