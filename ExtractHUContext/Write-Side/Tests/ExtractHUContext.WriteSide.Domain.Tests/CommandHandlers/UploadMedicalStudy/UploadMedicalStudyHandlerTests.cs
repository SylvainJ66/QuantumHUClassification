using ExtractHUContext.WriteSide.Domain.CommandHandlers.UploadMedicalStudy;
using ExtractHUContext.WriteSide.Domain.CommandHandlers.UploadMedicalStudy.Commands;
using ExtractHUContext.WriteSide.Domain.Tests.CommandHandlers.UploadMedicalStudy.Stubs;
using FluentAssertions;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.Tests.CommandHandlers.UploadMedicalStudy;

public class UploadMedicalStudyHandlerTests
{
    private readonly FileStorageServiceStub _fileStorageService;
    private readonly MedicalStudyRepositoryStub _repository;
    private readonly DateTimeProviderStub _dateTimeProvider;
    private readonly DateTime _fixedDate;

    public UploadMedicalStudyHandlerTests()
    {
        _fileStorageService = new FileStorageServiceStub();
        _repository = new MedicalStudyRepositoryStub();
        _dateTimeProvider = new DateTimeProviderStub();
        _fixedDate = new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.SetNow(_fixedDate);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSucceed()
    {
        var studyId = Guid.NewGuid();
        var command = CreateValidCommand(studyId);

        var result = await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUploadFileToStorage()
    {
        var studyId = Guid.NewGuid();
        var command = CreateValidCommand(studyId);
        var expectedStorageKey = $"studies/{studyId}/{studyId}.zip";

        await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        _fileStorageService.UploadFileCalls.Should().HaveCount(1);
        var uploadCall = _fileStorageService.UploadFileCalls[0];
        uploadCall.ObjectKey.Should().Be(expectedStorageKey);
        uploadCall.FileStream.Should().BeSameAs(command.FileStream);
        uploadCall.ContentType.Should().Be(command.ContentType);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSaveMedicalStudyToRepository()
    {
        var studyId = Guid.NewGuid();
        var command = CreateValidCommand(studyId);

        await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        _repository.SavedStudies.Should().HaveCount(1);
        var savedStudy = _repository.SavedStudies[0];
        var snapshot = savedStudy.ToSnapshot();

        snapshot.Id.Should().Be(studyId);
        snapshot.FileName.Should().Be(command.FileName);
        snapshot.ContentType.Should().Be(command.ContentType);
        snapshot.FileSizeBytes.Should().Be(command.FileSizeBytes);
        snapshot.StorageKey.Should().Be($"studies/{studyId}/{studyId}.zip");
        snapshot.UploadDate.Should().Be(_fixedDate);
    }

    [Fact]
    public async Task Handle_WhenFileStorageFails_ShouldReturnFailure()
    {
        var command = CreateValidCommand();
        var storageError = "Storage service unavailable";
        _fileStorageService.SetUploadFileResult(Result.Failure<string>(storageError));

        var result = await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(storageError);
    }

    [Fact]
    public async Task Handle_WhenFileStorageFails_ShouldNotSaveToRepository()
    {
        var command = CreateValidCommand();
        _fileStorageService.SetUploadFileResult(Result.Failure<string>("Storage error"));

        await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        _repository.SavedStudies.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithEmptyFileName_ShouldReturnFailure()
    {
        var command = CreateValidCommand() with { FileName = "" };

        var result = await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("File name cannot be empty");
    }

    [Fact]
    public async Task Handle_WithWhitespaceFileName_ShouldReturnFailure()
    {
        var command = CreateValidCommand() with { FileName = "   " };

        var result = await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("File name cannot be empty");
    }

    [Fact]
    public async Task Handle_WithFileNameTooLong_ShouldReturnFailure()
    {
        var longFileName = new string('a', 256) + ".zip";
        var command = CreateValidCommand() with { FileName = longFileName };

        var result = await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("File name cannot exceed 255 characters");
    }

    [Fact]
    public async Task Handle_WithZeroFileSize_ShouldReturnFailure()
    {
        var command = CreateValidCommand() with { FileSizeBytes = 0 };

        var result = await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("File size must be greater than 0");
    }

    [Fact]
    public async Task Handle_WithNegativeFileSize_ShouldReturnFailure()
    {
        var command = CreateValidCommand() with { FileSizeBytes = -100 };

        var result = await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("File size must be greater than 0");
    }

    [Fact]
    public async Task Handle_WithFileSizeExceeding500MB_ShouldReturnFailure()
    {
        var fileSizeOver500MB = (500 * 1024 * 1024) + 1;
        var command = CreateValidCommand() with { FileSizeBytes = fileSizeOver500MB };

        var result = await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("File size cannot exceed 500 MB");
    }

    [Fact]
    public async Task Handle_WithFileSizeExactly500MB_ShouldSucceed()
    {
        var fileSizeExactly500MB = 500 * 1024 * 1024;
        var command = CreateValidCommand() with { FileSizeBytes = fileSizeExactly500MB };

        var result = await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ShouldNotSaveToRepository()
    {
        var command = CreateValidCommand() with { FileName = "" };

        await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        _repository.SavedStudies.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldGenerateCorrectStorageKey()
    {
        var studyId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var command = CreateValidCommand(studyId);
        var expectedStorageKey = "studies/12345678-1234-1234-1234-123456789abc/12345678-1234-1234-1234-123456789abc.zip";

        await UploadMedicalStudyHandler.Handle(
            command,
            _fileStorageService,
            _repository,
            _dateTimeProvider);

        _fileStorageService.UploadFileCalls[0].ObjectKey.Should().Be(expectedStorageKey);
        _repository.SavedStudies[0].ToSnapshot().StorageKey.Should().Be(expectedStorageKey);
    }

    private static UploadMedicalStudyCommand CreateValidCommand(Guid? studyId = null)
    {
        return new UploadMedicalStudyCommand(
            studyId ?? Guid.NewGuid(),
            "test-study.zip",
            "application/zip",
            1024 * 1024, // 1 MB
            new MemoryStream()
        );
    }
}
