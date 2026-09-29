namespace Portfolio.Services;

public interface IStorageService
{
    Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        string kind,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string? url,
        CancellationToken cancellationToken = default);
}