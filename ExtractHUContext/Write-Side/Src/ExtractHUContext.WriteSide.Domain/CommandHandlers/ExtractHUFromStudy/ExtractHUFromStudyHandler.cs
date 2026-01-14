using ExtractHUContext.WriteSide.Domain.CommandHandlers.ExtractHUFromStudy.Commands;
using ExtractHUContext.WriteSide.Domain.Models;
using ExtractHUContext.WriteSide.Domain.Models.ValueObjects;
using ExtractHUContext.WriteSide.Domain.Ports;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.CommandHandlers.ExtractHUFromStudy;

/// <summary>
/// UseCase for extracting Hounsfield Units (HU) statistics from a medical study file stored in the bucket.
/// </summary>
public static class ExtractHuFromStudyHandler
{
    public static async Task<Result> Handle(
        ExtractHuFromStudyCommand command,
        IMedicalStudyRepository medicalStudyRepository,
        IFileStorageService fileStorageService,
        IHuExtractionService huExtractionService,
        IDateTimeProvider dateTimeProvider)
    {
        var loadStudyResult = await LoadStudy(command, medicalStudyRepository);
        if (!loadStudyResult.IsSuccess)
            return Result.Failure(loadStudyResult.Error);

        var study = loadStudyResult.Value;

        var startExtractionResult = StartExtraction(study, dateTimeProvider);
        if (!startExtractionResult.IsSuccess)
            return Result.Failure(startExtractionResult.Error);

        await SaveStudy(medicalStudyRepository, study);

        var extractionResult = await PerformExtraction(study, fileStorageService, huExtractionService);

        if (extractionResult.IsSuccess)
        {
            var completeResult = CompleteExtraction(study, extractionResult.Value, dateTimeProvider);
            if (!completeResult.IsSuccess)
                return Result.Failure(completeResult.Error);
        }
        else
        {
            var failResult = FailExtraction(study, extractionResult.Error, dateTimeProvider);
            if (!failResult.IsSuccess)
                return Result.Failure(failResult.Error);
        }

        await SaveStudy(medicalStudyRepository, study);

        return extractionResult.IsSuccess
            ? Result.Success()
            : Result.Failure(extractionResult.Error);
    }

    private static async Task<Result<MedicalStudy>> LoadStudy(
        ExtractHuFromStudyCommand command,
        IMedicalStudyRepository medicalStudyRepository)
    {
        var study = await medicalStudyRepository.GetById(command.StudyId);

        if (study == null)
            return Result.Failure<MedicalStudy>("Medical study not found");

        return Result.Success(study);
    }

    private static Result StartExtraction(
        MedicalStudy study,
        IDateTimeProvider dateTimeProvider)
        => study.StartExtraction(dateTimeProvider);

    private static async Task<Result<HuStatistics>> PerformExtraction(
        MedicalStudy study,
        IFileStorageService fileStorageService,
        IHuExtractionService huExtractionService)
    {
        var snapshot = study.ToSnapshot();
        var downloadResult = await fileStorageService.DownloadFile(snapshot.StorageKey);

        if (!downloadResult.IsSuccess)
            return Result.Failure<HuStatistics>($"Failed to download file: {downloadResult.Error}");

        await using var fileStream = downloadResult.Value;
        return await huExtractionService.ExtractHuStatisticsAsync(fileStream);
    }

    private static Result CompleteExtraction(
        MedicalStudy study,
        HuStatistics huStatistics,
        IDateTimeProvider dateTimeProvider)
        => study.CompleteExtraction(huStatistics, dateTimeProvider);

    private static Result FailExtraction(
        MedicalStudy study,
        string errorMessage,
        IDateTimeProvider dateTimeProvider)
        => study.FailExtraction(errorMessage, dateTimeProvider);

    private static async Task SaveStudy(
        IMedicalStudyRepository medicalStudyRepository,
        MedicalStudy study)
        => await medicalStudyRepository.Save(study);
}
