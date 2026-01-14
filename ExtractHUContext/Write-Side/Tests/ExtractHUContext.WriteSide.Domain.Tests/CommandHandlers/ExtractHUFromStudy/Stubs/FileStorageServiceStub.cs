using ExtractHUContext.WriteSide.Domain.Ports;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.Tests.CommandHandlers.ExtractHUFromStudy.Stubs;

public class FileStorageServiceStub : IFileStorageService
{
    private Result<Stream>? _downloadFileResult;

    public void SetDownloadFileResult(Result<Stream> result)
    {
        _downloadFileResult = result;
    }

    public List<string> DownloadFileCalls { get; } = new();

    public Task<Result<string>> UploadFile(
        string objectKey,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteFile(string objectKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Stream>> DownloadFile(string objectKey, CancellationToken cancellationToken = default)
    {
        DownloadFileCalls.Add(objectKey);
        return Task.FromResult(_downloadFileResult ?? Result.Success<Stream>(new MemoryStream()));
    }
}
