using ExtractHUContext.WriteSide.Domain.CommandHandlers.UploadMedicalStudy.Commands;
using ExtractHUContext.WriteSide.Domain.Models;
using ExtractHUContext.WriteSide.Domain.Ports;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.CommandHandlers.UploadMedicalStudy;

/// <summary>
/// UseCase for uploading a medical study file and creating the corresponding medical study record.
/// </summary>
public static class UploadMedicalStudyHandler
{
    public static async Task<Result> Handle(
        UploadMedicalStudyCommand command,
        IFileStorageService fileStorageService,
        IMedicalStudyRepository medicalStudyRepository,
        IDateTimeProvider dateTimeProvider)
    {
        var storageKey = BuildStorageKey(command);

        var uploadResult = await UploadFileToBucket(command, fileStorageService, storageKey);
        
        if (!uploadResult.IsSuccess)
            return Result.Failure(uploadResult.Error);

        var createStudyResult = CreateMedicalStudy(command, dateTimeProvider, storageKey);
        
        if (!createStudyResult.IsSuccess)
            return Result.Failure(createStudyResult.Error);

        await PersistMedicalStudy(medicalStudyRepository, createStudyResult);

        return Result.Success();
    }

    private static string BuildStorageKey(
        UploadMedicalStudyCommand command) 
        => $"studies/{command.StudyId}/{command.StudyId}.zip";

    private static async Task<Result<string>> UploadFileToBucket(
        UploadMedicalStudyCommand command,
        IFileStorageService fileStorageService,
        string storageKey) 
        => await fileStorageService.UploadFile(
            storageKey,
            command.FileStream,
            command.ContentType);

    private static Result<MedicalStudy> CreateMedicalStudy(
        UploadMedicalStudyCommand command,
        IDateTimeProvider dateTimeProvider,
        string storageKey) 
        => MedicalStudy.Create(
            command.StudyId,
            command.FileName,
            command.ContentType,
            command.FileSizeBytes,
            storageKey,
            dateTimeProvider);

    private static async Task PersistMedicalStudy(
        IMedicalStudyRepository medicalStudyRepository,
        Result<MedicalStudy> createStudyResult) 
        => await medicalStudyRepository.Save(createStudyResult.Value);
}
