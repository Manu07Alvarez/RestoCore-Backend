namespace RestoCore.IntegrationTests.Storage;

using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestoCore.Application.Common.Interfaces;
using RestoCore.IntegrationTests.Fixtures;
using Xunit;

public class SeaweedStorageTests : IClassFixture<ContainerizedStackFixture>
{
    private readonly ContainerizedStackFixture _factory;

    public SeaweedStorageTests(ContainerizedStackFixture factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PresignedUrl_DirectUploadAndPublicRead_SucceedsWithoutApiBuffering()
    {
        var storageService = _factory.Services.GetRequiredService<IStorageService>();
        var tenantId = Guid.NewGuid();

        // 1. Generate Presigned URL
        var presigned = await storageService.GeneratePresignedUploadUrlAsync(
            tenantId,
            "bife-chorizo.webp",
            "image/webp",
            "dishes");

        presigned.Should().NotBeNull();
        presigned.UploadUrl.Should().Contain("restocore-images");
        presigned.PublicUrl.Should().Contain("restocore-images");

        // 2. Perform direct HTTP PUT with binary data directly to SeaweedFS (bypassing API)
        using var s3HttpClient = new HttpClient();
        var fakeBinaryContent = new ByteArrayContent(Encoding.UTF8.GetBytes("RIFF....WEBPVP8 ..."));
        fakeBinaryContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/webp");

        var uploadResponse = await s3HttpClient.PutAsync(presigned.UploadUrl, fakeBinaryContent);

        // SeaweedFS S3 gateway accepts PUT and returns OK / Created
        uploadResponse.IsSuccessStatusCode.Should().BeTrue();

        // 3. Verify public GET can retrieve the uploaded object
        var readResponse = await s3HttpClient.GetAsync(presigned.PublicUrl);
        readResponse.IsSuccessStatusCode.Should().BeTrue();

        var downloadedBytes = await readResponse.Content.ReadAsByteArrayAsync();
        downloadedBytes.Length.Should().BeGreaterThan(0);
    }
}
