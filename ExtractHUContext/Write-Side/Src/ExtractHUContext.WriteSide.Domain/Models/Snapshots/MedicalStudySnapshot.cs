using ExtractHUContext.WriteSide.Domain.Models.ValueObjects;

namespace ExtractHUContext.WriteSide.Domain.Models.Snapshots;

public record MedicalStudySnapshot(
    Guid Id,
    DateTime UploadDate,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StorageKey,
    ExtractionStatus ExtractionStatus,
    DateTime? ExtractionStartedAt,
    DateTime? ExtractionCompletedAt,
    HuStatistics? HuStatistics,
    string? ExtractionError
);
