namespace ExtractHUContext.WriteSide.Domain.CommandHandlers.UploadMedicalStudy.Commands;

public record UploadMedicalStudyCommand(
    Guid StudyId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    Stream FileStream
);
