namespace ExtractHUContext.ReadSide.Domain.ReadModels;

public record MedicalStudyReadModel(
    Guid Id,
    DateTime UploadDate,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StorageKey
);
