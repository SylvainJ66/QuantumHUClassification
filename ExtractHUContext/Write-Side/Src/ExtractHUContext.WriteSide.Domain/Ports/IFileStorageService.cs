using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.Ports;

public interface IFileStorageService
{
    Task<Result<string>> UploadFile(
        string objectKey,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteFile(
        string objectKey,
        CancellationToken cancellationToken = default);

    Task<Result<Stream>> DownloadFile(
        string objectKey,
        CancellationToken cancellationToken = default);
}
