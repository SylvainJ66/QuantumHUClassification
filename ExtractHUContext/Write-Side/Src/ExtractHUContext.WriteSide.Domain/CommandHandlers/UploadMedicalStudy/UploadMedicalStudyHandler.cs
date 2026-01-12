using ExtractHUContext.WriteSide.Domain.CommandHandlers.UploadMedicalStudy.Commands;
using ExtractHUContext.WriteSide.Domain.Models;
using ExtractHUContext.WriteSide.Domain.Ports;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.CommandHandlers.UploadMedicalStudy;

public static class UploadMedicalStudyHandler
{
    public static async Task<Result> Handle(
        UploadMedicalStudyCommand command,
        IFileStorageService fileStorageService,
        IMedicalStudyRepository medicalStudyRepository,
        IDateTimeProvider dateTimeProvider)
    {
        // Step 1: Generate storage key
        var storageKey = $"studies/{command.StudyId}/{command.StudyId}.zip";

        // Step 2: Upload file to MinIO
        var uploadResult = await fileStorageService.UploadFile(
            storageKey,
            command.FileStream,
            command.ContentType);

        if (!uploadResult.IsSuccess)
            return Result.Failure(uploadResult.Error);

        // Step 3: Create domain model
        var createStudyResult = MedicalStudy.Create(
            command.StudyId,
            command.FileName,
            command.ContentType,
            command.FileSizeBytes,
            storageKey,
            dateTimeProvider);

        if (!createStudyResult.IsSuccess)
            return Result.Failure(createStudyResult.Error);

        // Step 4: Save to database
        // If this fails, file remains in MinIO (no compensation as per requirement)
        await medicalStudyRepository.Save(createStudyResult.Value);

        return Result.Success();
    }
}
