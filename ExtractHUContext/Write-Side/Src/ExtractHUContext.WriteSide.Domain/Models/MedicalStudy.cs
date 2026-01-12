using ExtractHUContext.WriteSide.Domain.Models.Snapshots;
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

    private MedicalStudy(
        Guid id,
        DateTime uploadDate,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storageKey)
    {
        _id = id;
        _uploadDate = uploadDate;
        _fileName = fileName;
        _contentType = contentType;
        _fileSizeBytes = fileSizeBytes;
        _storageKey = storageKey;
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
            storageKey);

        return Result.Success(study);
    }

    public MedicalStudySnapshot ToSnapshot()
    {
        return new MedicalStudySnapshot(
            _id,
            _uploadDate,
            _fileName,
            _contentType,
            _fileSizeBytes,
            _storageKey
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
            snapshot.StorageKey
        );
    }
}
