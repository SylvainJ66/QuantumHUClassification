using ExtractHUContext.WriteSide.Infrastructure.Storage;
using ExtractHUContext.WriteSide.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ExtractHUContext.WriteSide.Infrastructure.Tests.Storage;

public class MinioFileStorageServiceTests : IClassFixture<MinioFixture>
{
    private readonly MinioFixture _minioFixture;

    public MinioFileStorageServiceTests(MinioFixture minioFixture)
    {
        _minioFixture = minioFixture;
    }

    [Fact]
    public async Task UploadFile_WithValidFile_ShouldUploadSuccessfully()
    {
        var service = CreateService();
        var objectKey = "test-folder/test-file.txt";
        var content = "Hello, MinIO!"u8.ToArray();
        using var stream = new MemoryStream(content);

        var result = await service.UploadFile(objectKey, stream, "text/plain");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(objectKey);
    }

    [Fact]
    public async Task UploadFile_ShouldCreateBucketIfNotExists()
    {
        var service = CreateService();
        var objectKey = "new-bucket-test/file.txt";
        var content = "Test content"u8.ToArray();
        using var stream = new MemoryStream(content);

        var result = await service.UploadFile(objectKey, stream, "text/plain");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UploadFile_WithBinaryContent_ShouldUploadSuccessfully()
    {
        var service = CreateService();
        var objectKey = "binary/test.bin";
        var binaryContent = new byte[] { 0x00, 0xFF, 0x7F, 0x80, 0x01 };
        using var stream = new MemoryStream(binaryContent);

        var result = await service.UploadFile(objectKey, stream, "application/octet-stream");

        result.IsSuccess.Should().BeTrue();

        var downloadResult = await service.DownloadFile(objectKey);
        downloadResult.IsSuccess.Should().BeTrue();
        using var downloadedStream = downloadResult.Value;
        using var ms = new MemoryStream();
        await downloadedStream.CopyToAsync(ms);
        ms.ToArray().Should().BeEquivalentTo(binaryContent);
    }

    [Fact]
    public async Task UploadFile_WithLargeFile_ShouldUploadSuccessfully()
    {
        var service = CreateService();
        var objectKey = "large/test.dat";
        var size = 5 * 1024 * 1024; // 5 MB
        var largeContent = new byte[size];
        Random.Shared.NextBytes(largeContent);
        using var stream = new MemoryStream(largeContent);

        var result = await service.UploadFile(objectKey, stream, "application/octet-stream");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DownloadFile_WhenFileExists_ShouldReturnContent()
    {
        var service = CreateService();
        var objectKey = "download-test/file.txt";
        var expectedContent = "Download test content"u8.ToArray();
        using var uploadStream = new MemoryStream(expectedContent);

        await service.UploadFile(objectKey, uploadStream, "text/plain");

        var downloadResult = await service.DownloadFile(objectKey);

        downloadResult.IsSuccess.Should().BeTrue();
        using var downloadedStream = downloadResult.Value;
        using var ms = new MemoryStream();
        await downloadedStream.CopyToAsync(ms);
        ms.ToArray().Should().BeEquivalentTo(expectedContent);
    }

    [Fact]
    public async Task DownloadFile_WhenFileDoesNotExist_ShouldReturnFailure()
    {
        var service = CreateService();
        var nonExistentKey = "non-existent/file.txt";

        var result = await service.DownloadFile(nonExistentKey);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to download file");
    }

    [Fact]
    public async Task DeleteFile_WhenFileExists_ShouldDeleteSuccessfully()
    {
        var service = CreateService();
        var objectKey = "delete-test/file.txt";
        var content = "File to delete"u8.ToArray();
        using var stream = new MemoryStream(content);

        await service.UploadFile(objectKey, stream, "text/plain");

        var deleteResult = await service.DeleteFile(objectKey);

        deleteResult.IsSuccess.Should().BeTrue();

        var downloadResult = await service.DownloadFile(objectKey);
        downloadResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFile_WhenFileDoesNotExist_ShouldNotFail()
    {
        var service = CreateService();
        var nonExistentKey = "delete-test/non-existent.txt";

        var result = await service.DeleteFile(nonExistentKey);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UploadFile_WithNestedPath_ShouldUploadSuccessfully()
    {
        var service = CreateService();
        var objectKey = "level1/level2/level3/deeply-nested.txt";
        var content = "Nested content"u8.ToArray();
        using var stream = new MemoryStream(content);

        var result = await service.UploadFile(objectKey, stream, "text/plain");

        result.IsSuccess.Should().BeTrue();

        var downloadResult = await service.DownloadFile(objectKey);
        downloadResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UploadFile_WithSpecialCharactersInKey_ShouldUploadSuccessfully()
    {
        var service = CreateService();
        var objectKey = "special/file-with-special_chars.2026.txt";
        var content = "Special chars test"u8.ToArray();
        using var stream = new MemoryStream(content);

        var result = await service.UploadFile(objectKey, stream, "text/plain");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UploadFile_OverwriteExistingFile_ShouldReplaceContent()
    {
        var service = CreateService();
        var objectKey = "overwrite-test/file.txt";

        var originalContent = "Original content"u8.ToArray();
        using (var stream = new MemoryStream(originalContent))
        {
            await service.UploadFile(objectKey, stream, "text/plain");
        }

        var newContent = "New content"u8.ToArray();
        using (var stream = new MemoryStream(newContent))
        {
            await service.UploadFile(objectKey, stream, "text/plain");
        }

        var downloadResult = await service.DownloadFile(objectKey);
        downloadResult.IsSuccess.Should().BeTrue();
        using var downloadedStream = downloadResult.Value;
        using var ms = new MemoryStream();
        await downloadedStream.CopyToAsync(ms);
        ms.ToArray().Should().BeEquivalentTo(newContent);
    }

    [Fact]
    public async Task UploadFile_WithZipFile_ShouldPreserveContent()
    {
        var service = CreateService();
        var objectKey = "medical-studies/study-123.zip";

        var zipContent = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // ZIP file header
        using var stream = new MemoryStream(zipContent);

        var uploadResult = await service.UploadFile(objectKey, stream, "application/zip");
        uploadResult.IsSuccess.Should().BeTrue();

        var downloadResult = await service.DownloadFile(objectKey);
        downloadResult.IsSuccess.Should().BeTrue();
        using var downloadedStream = downloadResult.Value;
        using var ms = new MemoryStream();
        await downloadedStream.CopyToAsync(ms);
        ms.ToArray().Should().StartWith(zipContent);
    }

    private MinioFileStorageService CreateService()
    {
        var options = Options.Create(new MinioOptions
        {
            Endpoint = _minioFixture.GetEndpoint(),
            AccessKey = _minioFixture.AccessKey,
            SecretKey = _minioFixture.SecretKey,
            BucketName = $"test-bucket-{Guid.NewGuid():N}",
            UseSSL = false
        });

        var logger = NullLogger<MinioFileStorageService>.Instance;
        var minioClient = _minioFixture.CreateMinioClient();

        return new MinioFileStorageService(minioClient, options, logger);
    }
}
