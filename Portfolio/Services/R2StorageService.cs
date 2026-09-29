using Amazon.S3;
using Amazon.S3.Model;

namespace Portfolio.Services;

public sealed class R2StorageService : IStorageService
{
    private readonly IAmazonS3 _client;
    private readonly IConfiguration _configuration;

    public R2StorageService(
        IAmazonS3 client,
        IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        string kind,
        CancellationToken cancellationToken = default)
    {
        var bucket = _configuration["R2:Bucket"]
            ?? throw new InvalidOperationException("R2:Bucket não configurado.");

        var publicBaseUrl = _configuration["R2:PublicBaseUrl"]
            ?? throw new InvalidOperationException("R2:PublicBaseUrl não configurado.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var objectName = $"portfolio-files/{kind}/{Guid.NewGuid():N}{extension}";

        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = objectName,
            InputStream = content,
            ContentType = contentType,
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true
        };

        await _client.PutObjectAsync(request, cancellationToken);

        return $"{publicBaseUrl.TrimEnd('/')}/{objectName}";
    }

    public async Task DeleteAsync(
        string? url,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        var publicBaseUrl = _configuration["R2:PublicBaseUrl"];

        if (string.IsNullOrWhiteSpace(publicBaseUrl))
            return;

        if (!url.StartsWith(publicBaseUrl, StringComparison.OrdinalIgnoreCase))
            return;

        var objectKey = url[publicBaseUrl.Length..].TrimStart('/');

        if (string.IsNullOrWhiteSpace(objectKey))
            return;

        var bucket = _configuration["R2:Bucket"]
            ?? throw new InvalidOperationException("R2:Bucket não configurado.");

        await _client.DeleteObjectAsync(
            new DeleteObjectRequest
            {
                BucketName = bucket,
                Key = objectKey
            },
            cancellationToken);
    }
}