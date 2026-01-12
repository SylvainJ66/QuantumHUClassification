namespace ExtractHUContext.WriteSide.Domain.Models.Snapshots;

public record MedicalStudySnapshot(
    Guid Id,
    DateTime UploadDate,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StorageKey
);
