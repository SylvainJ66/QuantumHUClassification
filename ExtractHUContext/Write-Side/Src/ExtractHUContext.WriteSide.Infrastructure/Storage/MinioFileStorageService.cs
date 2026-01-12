using ExtractHUContext.WriteSide.Domain.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Infrastructure.Storage;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioOptions _options;
    private readonly ILogger<MinioFileStorageService> _logger;

    public MinioFileStorageService(
        IMinioClient minioClient,
        IOptions<MinioOptions> options,
        ILogger<MinioFileStorageService> logger)
    {
        _minioClient = minioClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<string>> UploadFile(
        string objectKey,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure bucket exists
            var bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_options.BucketName),
                cancellationToken);

            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_options.BucketName),
                    cancellationToken);
            }

            // Upload file
            await _minioClient.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(objectKey)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType(contentType),
                cancellationToken);

            _logger.LogInformation("File uploaded successfully: {ObjectKey}", objectKey);
            return Result.Success(objectKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file: {ObjectKey}", objectKey);
            return Result.Failure<string>($"Failed to upload file: {ex.Message}");
        }
    }

    public async Task<Result> DeleteFile(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _minioClient.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(objectKey),
                cancellationToken);

            _logger.LogInformation("File deleted successfully: {ObjectKey}", objectKey);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {ObjectKey}", objectKey);
            return Result.Failure($"Failed to delete file: {ex.Message}");
        }
    }

    public async Task<Result<Stream>> DownloadFile(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var memoryStream = new MemoryStream();

            await _minioClient.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(objectKey)
                    .WithCallbackStream(stream => stream.CopyTo(memoryStream)),
                cancellationToken);

            memoryStream.Position = 0;
            _logger.LogInformation("File downloaded successfully: {ObjectKey}", objectKey);
            return Result.Success<Stream>(memoryStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file: {ObjectKey}", objectKey);
            return Result.Failure<Stream>($"Failed to download file: {ex.Message}");
        }
    }
}
