namespace RestoCore.Infrastructure.Storage;

using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using RestoCore.Application.Common.Interfaces;

public class SeaweedStorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicBaseUrl;

    public SeaweedStorageService(IConfiguration configuration)
    {
        var serviceUrl = configuration["Storage:ServiceUrl"] ?? "http://localhost:8333";
        _publicBaseUrl = configuration["Storage:PublicBaseUrl"] ?? serviceUrl;
        _bucketName = configuration["Storage:BucketName"] ?? "restocore-images";
        var accessKey = configuration["Storage:AccessKey"] ?? "any";
        var secretKey = configuration["Storage:SecretKey"] ?? "any";

        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true,
            UseHttp = true // Strictly use HTTP to prevent SSL handshakes against local SeaweedFS
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<PresignedUploadResponse> GeneratePresignedUploadUrlAsync(
        Guid tenantId,
        string fileName,
        string contentType,
        string category,
        CancellationToken cancellationToken = default)
    {
        var sanitizedFileName = Path.GetFileName(fileName);
        var key = $"tenants/{tenantId}/{category}/{Guid.NewGuid():N}_{sanitizedFileName}";
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = expiresAt,
            ContentType = contentType,
            Protocol = Protocol.HTTP // Ensure generated presigned URL uses HTTP
        };

        var presignedUrl = await Task.Run(() => _s3Client.GetPreSignedURL(request), cancellationToken);
        var publicUrl = $"{_publicBaseUrl.TrimEnd('/')}/{_bucketName}/{key}";

        return new PresignedUploadResponse(presignedUrl, publicUrl, expiresAt);
    }

    public async Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _s3Client.ListBucketsAsync(cancellationToken);
            return response.Buckets.Any(b => string.Equals(b.BucketName, bucketName, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }
}
