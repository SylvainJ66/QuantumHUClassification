using ExtractHUContext.WriteSide.Domain.Models.Snapshots;
using ExtractHUContext.WriteSide.Domain.Models.ValueObjects;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.Models;

public class MedicalStudy
{
    private readonly Guid _id;
    private readonly DateTime _uploadDate;
    private readonly string _fileName;
    private readonly string _contentType;
    private readonly long _fileSizeBytes;
    private readonly string _storageKey;

    private ExtractionStatus _extractionStatus;
    private DateTime? _extractionStartedAt;
    private DateTime? _extractionCompletedAt;
    private HuStatistics? _huStatistics;
    private string? _extractionError;

    private MedicalStudy(
        Guid id,
        DateTime uploadDate,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storageKey,
        ExtractionStatus extractionStatus,
        DateTime? extractionStartedAt,
        DateTime? extractionCompletedAt,
        HuStatistics? huStatistics,
        string? extractionError)
    {
        _id = id;
        _uploadDate = uploadDate;
        _fileName = fileName;
        _contentType = contentType;
        _fileSizeBytes = fileSizeBytes;
        _storageKey = storageKey;
        _extractionStatus = extractionStatus;
        _extractionStartedAt = extractionStartedAt;
        _extractionCompletedAt = extractionCompletedAt;
        _huStatistics = huStatistics;
        _extractionError = extractionError;
    }

    public static Result<MedicalStudy> Create(
        Guid id,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storageKey,
        IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Failure<MedicalStudy>("File name cannot be empty");

        if (fileName.Length > 255)
            return Result.Failure<MedicalStudy>("File name cannot exceed 255 characters");

        if (fileSizeBytes <= 0)
            return Result.Failure<MedicalStudy>("File size must be greater than 0");

        if (fileSizeBytes > 500 * 1024 * 1024) // 500 MB
            return Result.Failure<MedicalStudy>("File size cannot exceed 500 MB");

        if (string.IsNullOrWhiteSpace(storageKey))
            return Result.Failure<MedicalStudy>("Storage key cannot be empty");

        var study = new MedicalStudy(
            id,
            dateTimeProvider.Now,
            fileName,
            contentType,
            fileSizeBytes,
            storageKey,
            ExtractionStatus.Pending,
            null,
            null,
            null,
            null);

        return Result.Success(study);
    }

    public Result StartExtraction(IDateTimeProvider dateTimeProvider)
    {
        if (_extractionStatus == ExtractionStatus.InProgress)
            return Result.Failure("Extraction is already in progress");

        if (_extractionStatus == ExtractionStatus.Completed)
            return Result.Failure("Extraction has already been completed");

        _extractionStatus = ExtractionStatus.InProgress;
        _extractionStartedAt = dateTimeProvider.Now;
        _extractionError = null;

        return Result.Success();
    }

    public Result CompleteExtraction(HuStatistics huStatistics, IDateTimeProvider dateTimeProvider)
    {
        if (_extractionStatus != ExtractionStatus.InProgress)
            return Result.Failure("Extraction must be in progress to complete it");

        _extractionStatus = ExtractionStatus.Completed;
        _extractionCompletedAt = dateTimeProvider.Now;
        _huStatistics = huStatistics;
        _extractionError = null;

        return Result.Success();
    }

    public Result FailExtraction(string errorMessage, IDateTimeProvider dateTimeProvider)
    {
        if (_extractionStatus != ExtractionStatus.InProgress)
            return Result.Failure("Extraction must be in progress to fail it");

        if (string.IsNullOrWhiteSpace(errorMessage))
            return Result.Failure("Error message cannot be empty");

        _extractionStatus = ExtractionStatus.Failed;
        _extractionCompletedAt = dateTimeProvider.Now;
        _extractionError = errorMessage;

        return Result.Success();
    }

    public MedicalStudySnapshot ToSnapshot()
    {
        return new MedicalStudySnapshot(
            _id,
            _uploadDate,
            _fileName,
            _contentType,
            _fileSizeBytes,
            _storageKey,
            _extractionStatus,
            _extractionStartedAt,
            _extractionCompletedAt,
            _huStatistics,
            _extractionError
        );
    }

    public static MedicalStudy FromSnapshot(MedicalStudySnapshot snapshot)
    {
        return new MedicalStudy(
            snapshot.Id,
            snapshot.UploadDate,
            snapshot.FileName,
            snapshot.ContentType,
            snapshot.FileSizeBytes,
            snapshot.StorageKey,
            snapshot.ExtractionStatus,
            snapshot.ExtractionStartedAt,
            snapshot.ExtractionCompletedAt,
            snapshot.HuStatistics,
            snapshot.ExtractionError
        );
    }
}
