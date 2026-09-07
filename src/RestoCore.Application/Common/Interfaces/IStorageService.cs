namespace RestoCore.Application.Common.Interfaces;

public record PresignedUploadResponse(
    string UploadUrl,
    string PublicUrl,
    DateTimeOffset ExpiresAt
);

public interface IStorageService
{
    Task<PresignedUploadResponse> GeneratePresignedUploadUrlAsync(
        Guid tenantId,
        string fileName,
        string contentType,
        string category, // "dishes" or "branding"
        CancellationToken cancellationToken = default);

    Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default);
}
