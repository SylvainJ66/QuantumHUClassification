using ExtractHUContext.WriteSide.Domain.Ports;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.Tests.CommandHandlers.UploadMedicalStudy.Stubs;

public class FileStorageServiceStub : IFileStorageService
{
    private Result<string>? _uploadFileResult;

    public void SetUploadFileResult(Result<string> result)
    {
        _uploadFileResult = result;
    }

    public List<(string ObjectKey, Stream FileStream, string ContentType)> UploadFileCalls { get; } = new();

    public Task<Result<string>> UploadFile(
        string objectKey,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        UploadFileCalls.Add((objectKey, fileStream, contentType));
        return Task.FromResult(_uploadFileResult ?? Result.Success(objectKey));
    }

    public Task<Result> DeleteFile(string objectKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Stream>> DownloadFile(string objectKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
