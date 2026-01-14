using ExtractHUContext.WriteSide.Domain.CommandHandlers.ExtractHUFromStudy;
using ExtractHUContext.WriteSide.Domain.CommandHandlers.ExtractHUFromStudy.Commands;
using ExtractHUContext.WriteSide.Domain.Models;
using ExtractHUContext.WriteSide.Domain.Models.ValueObjects;
using ExtractHUContext.WriteSide.Domain.Tests.CommandHandlers.ExtractHUFromStudy.Stubs;
using FluentAssertions;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.Tests.CommandHandlers.ExtractHUFromStudy;

public class ExtractHuFromStudyHandlerTests
{
    private readonly MedicalStudyRepositoryStub _repository;
    private readonly FileStorageServiceStub _fileStorageService;
    private readonly HuExtractionServiceStub _huExtractionService;
    private readonly DateTimeProviderStub _dateTimeProvider;
    private readonly DateTime _fixedStartDate;

    public ExtractHuFromStudyHandlerTests()
    {
        _repository = new MedicalStudyRepositoryStub();
        _fileStorageService = new FileStorageServiceStub();
        _huExtractionService = new HuExtractionServiceStub();
        _dateTimeProvider = new DateTimeProviderStub();
        _fixedStartDate = new DateTime(2026, 1, 13, 10, 0, 0, DateTimeKind.Utc);
        new DateTime(2026, 1, 13, 10, 5, 0, DateTimeKind.Utc);
        _dateTimeProvider.SetNow(_fixedStartDate);
    }

    [Fact]
    public async Task Should_extract_hu_statistics_and_mark_completed_when_successful()
    {
        var studyId = Guid.NewGuid();
        var study = CreateUploadedStudy(studyId);
        _repository.AddStudy(study);
        var command = new ExtractHuFromStudyCommand(studyId);
        var expectedStats = HuStatistics.Create(100.0, -500.0, 800.0, 75.5, 500000).Value;
        _huExtractionService.SetExtractResult(Result.Success(expectedStats));

        var result = await ExtractHuFromStudyHandler.Handle(
            command,
            _repository,
            _fileStorageService,
            _huExtractionService,
            _dateTimeProvider);

        result.IsSuccess.Should().BeTrue();
        _repository.SavedStudies.Should().HaveCount(2);
        var finalStudy = _repository.SavedStudies[1];
        var snapshot = finalStudy.ToSnapshot();
        snapshot.ExtractionStatus.Should().Be(ExtractionStatus.Completed);
        snapshot.ExtractionStartedAt.Should().Be(_fixedStartDate);
        snapshot.ExtractionCompletedAt.Should().Be(_fixedStartDate);
        snapshot.HuStatistics.Should().NotBeNull();
        snapshot.HuStatistics!.MeanHu.Should().Be(100.0);
        snapshot.HuStatistics.MinHu.Should().Be(-500.0);
        snapshot.HuStatistics.MaxHu.Should().Be(800.0);
        snapshot.HuStatistics.StandardDeviation.Should().Be(75.5);
        snapshot.HuStatistics.VoxelCount.Should().Be(500000);
        snapshot.ExtractionError.Should().BeNull();
    }

    [Fact]
    public async Task Should_download_file_from_correct_storage_key_when_extracting()
    {
        var studyId = Guid.NewGuid();
        var study = CreateUploadedStudy(studyId);
        _repository.AddStudy(study);
        var command = new ExtractHuFromStudyCommand(studyId);
        var expectedStorageKey = $"studies/{studyId}/{studyId}.zip";

        await ExtractHuFromStudyHandler.Handle(
            command,
            _repository,
            _fileStorageService,
            _huExtractionService,
            _dateTimeProvider);

        _fileStorageService.DownloadFileCalls.Should().HaveCount(1);
        _fileStorageService.DownloadFileCalls[0].Should().Be(expectedStorageKey);
    }

    [Fact]
    public async Task Should_call_extraction_service_with_file_stream_when_extracting()
    {
        var studyId = Guid.NewGuid();
        var study = CreateUploadedStudy(studyId);
        _repository.AddStudy(study);
        var command = new ExtractHuFromStudyCommand(studyId);

        await ExtractHuFromStudyHandler.Handle(
            command,
            _repository,
            _fileStorageService,
            _huExtractionService,
            _dateTimeProvider);

        _huExtractionService.ExtractHuStatisticsCalls.Should().HaveCount(1);
        _huExtractionService.ExtractHuStatisticsCalls[0].Should().NotBeNull();
    }

    [Fact]
    public async Task Should_save_study_twice_when_successful_extraction()
    {
        var studyId = Guid.NewGuid();
        var study = CreateUploadedStudy(studyId);
        _repository.AddStudy(study);
        var command = new ExtractHuFromStudyCommand(studyId);

        await ExtractHuFromStudyHandler.Handle(
            command,
            _repository,
            _fileStorageService,
            _huExtractionService,
            _dateTimeProvider);

        _repository.SavedStudies.Should().HaveCount(2);
        var firstSave = _repository.SavedStudies[0].ToSnapshot();
        firstSave.ExtractionStatus.Should().Be(ExtractionStatus.InProgress);
        var secondSave = _repository.SavedStudies[1].ToSnapshot();
        secondSave.ExtractionStatus.Should().Be(ExtractionStatus.Completed);
    }

    [Fact]
    public async Task Should_fail_when_study_not_found()
    {
        var command = new ExtractHuFromStudyCommand(Guid.NewGuid());

        var result = await ExtractHuFromStudyHandler.Handle(
            command,
            _repository,
            _fileStorageService,
            _huExtractionService,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Medical study not found");
        _repository.SavedStudies.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_fail_when_extraction_already_in_progress()
    {
        var studyId = Guid.NewGuid();
        var study = CreateUploadedStudy(studyId);
        study.StartExtraction(_dateTimeProvider);
        _repository.AddStudy(study);
        var command = new ExtractHuFromStudyCommand(studyId);

        var result = await ExtractHuFromStudyHandler.Handle(
            command,
            _repository,
            _fileStorageService,
            _huExtractionService,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Extraction is already in progress");
        _repository.SavedStudies.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_fail_when_extraction_already_completed()
    {
        var studyId = Guid.NewGuid();
        var study = CreateUploadedStudy(studyId);
        var stats = HuStatistics.Create(50.0, -1000.0, 1000.0, 100.0, 1000000).Value;
        study.StartExtraction(_dateTimeProvider);
        study.CompleteExtraction(stats, _dateTimeProvider);
        _repository.AddStudy(study);
        var command = new ExtractHuFromStudyCommand(studyId);

        var result = await ExtractHuFromStudyHandler.Handle(
            command,
            _repository,
            _fileStorageService,
            _huExtractionService,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Extraction has already been completed");
        _repository.SavedStudies.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_mark_as_failed_when_file_download_fails()
    {
        var studyId = Guid.NewGuid();
        var study = CreateUploadedStudy(studyId);
        _repository.AddStudy(study);
        var command = new ExtractHuFromStudyCommand(studyId);
        var downloadError = "Storage service unavailable";
        _fileStorageService.SetDownloadFileResult(Result.Failure<Stream>(downloadError));
        _dateTimeProvider.SetNow(_fixedStartDate);

        var result = await ExtractHuFromStudyHandler.Handle(
            command,
            _repository,
            _fileStorageService,
            _huExtractionService,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(downloadError);
        _repository.SavedStudies.Should().HaveCount(2);
        var finalStudy = _repository.SavedStudies[1];
        var snapshot = finalStudy.ToSnapshot();
        snapshot.ExtractionStatus.Should().Be(ExtractionStatus.Failed);
        snapshot.ExtractionStartedAt.Should().Be(_fixedStartDate);
        snapshot.ExtractionCompletedAt.Should().Be(_fixedStartDate);
        snapshot.ExtractionError.Should().Contain(downloadError);
        snapshot.HuStatistics.Should().BeNull();
    }

    [Fact]
    public async Task Should_mark_as_failed_when_hu_extraction_fails()
    {
        var studyId = Guid.NewGuid();
        var study = CreateUploadedStudy(studyId);
        _repository.AddStudy(study);
        var command = new ExtractHuFromStudyCommand(studyId);
        var extractionError = "Invalid DICOM file format";
        _huExtractionService.SetExtractResult(Result.Failure<HuStatistics>(extractionError));

        var result = await ExtractHuFromStudyHandler.Handle(
            command,
            _repository,
            _fileStorageService,
            _huExtractionService,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(extractionError);
        _repository.SavedStudies.Should().HaveCount(2);
        var finalStudy = _repository.SavedStudies[1];
        var snapshot = finalStudy.ToSnapshot();
        snapshot.ExtractionStatus.Should().Be(ExtractionStatus.Failed);
        snapshot.ExtractionError.Should().Be(extractionError);
        snapshot.HuStatistics.Should().BeNull();
    }

    [Fact]
    public async Task Should_set_correct_timestamps_when_extraction_succeeds()
    {
        var studyId = Guid.NewGuid();
        var study = CreateUploadedStudy(studyId);
        _repository.AddStudy(study);
        var command = new ExtractHuFromStudyCommand(studyId);
        _dateTimeProvider.SetNow(_fixedStartDate);

        await ExtractHuFromStudyHandler.Handle(
            command,
            _repository,
            _fileStorageService,
            _huExtractionService,
            _dateTimeProvider);

        var inProgressStudy = _repository.SavedStudies[0].ToSnapshot();
        inProgressStudy.ExtractionStartedAt.Should().Be(_fixedStartDate);
        inProgressStudy.ExtractionCompletedAt.Should().BeNull();
        var completedStudy = _repository.SavedStudies[1].ToSnapshot();
        completedStudy.ExtractionStartedAt.Should().Be(_fixedStartDate);
        completedStudy.ExtractionCompletedAt.Should().Be(_fixedStartDate);
    }

    [Fact]
    public async Task Should_set_correct_timestamps_when_extraction_fails()
    {
        var studyId = Guid.NewGuid();
        var study = CreateUploadedStudy(studyId);
        _repository.AddStudy(study);
        var command = new ExtractHuFromStudyCommand(studyId);
        _huExtractionService.SetExtractResult(Result.Failure<HuStatistics>("Extraction failed"));
        _dateTimeProvider.SetNow(_fixedStartDate);

        await ExtractHuFromStudyHandler.Handle(
            command,
            _repository,
            _fileStorageService,
            _huExtractionService,
            _dateTimeProvider);

        var inProgressStudy = _repository.SavedStudies[0].ToSnapshot();
        inProgressStudy.ExtractionStartedAt.Should().Be(_fixedStartDate);
        inProgressStudy.ExtractionCompletedAt.Should().BeNull();
        var failedStudy = _repository.SavedStudies[1].ToSnapshot();
        failedStudy.ExtractionStartedAt.Should().Be(_fixedStartDate);
        failedStudy.ExtractionCompletedAt.Should().Be(_fixedStartDate);
    }

    private static MedicalStudy CreateUploadedStudy(Guid studyId)
    {
        var uploadDate = new DateTime(2026, 1, 13, 9, 0, 0, DateTimeKind.Utc);
        var uploadDateProvider = new DateTimeProviderStub();
        uploadDateProvider.SetNow(uploadDate);

        var study = MedicalStudy.Create(
            studyId,
            "brain-scan.zip",
            "application/zip",
            10 * 1024 * 1024,
            $"studies/{studyId}/{studyId}.zip",
            uploadDateProvider).Value;

        return study;
    }
}
