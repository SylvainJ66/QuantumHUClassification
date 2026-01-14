using ExtractHUContext.WriteSide.Domain.Models;
using ExtractHUContext.WriteSide.Domain.Models.ValueObjects;
using ExtractHUContext.WriteSide.Infrastructure.Persistence.Repositories;
using ExtractHUContext.WriteSide.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Infrastructure.Tests.Repositories;

public class EfMedicalStudyRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public EfMedicalStudyRepositoryTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task Save_WithNewStudy_ShouldPersistToDatabase()
    {
        await using var context = _databaseFixture.CreateDbContext();
        var repository = new EfMedicalStudyRepository(context);
        var dateTimeProvider = new FixedDateTimeProvider(new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc));

        var study = MedicalStudy.Create(
            Guid.NewGuid(),
            "test-study.zip",
            "application/zip",
            1024 * 1024,
            "studies/test.zip",
            dateTimeProvider).Value;

        await repository.Save(study);

        var savedStudy = await repository.GetById(study.ToSnapshot().Id);

        savedStudy.Should().NotBeNull();
        var snapshot = savedStudy.ToSnapshot();
        snapshot.FileName.Should().Be("test-study.zip");
        snapshot.ContentType.Should().Be("application/zip");
        snapshot.FileSizeBytes.Should().Be(1024 * 1024);
        snapshot.StorageKey.Should().Be("studies/test.zip");
    }

    [Fact]
    public async Task GetById_WhenStudyExists_ShouldReturnStudy()
    {
        await using var context = _databaseFixture.CreateDbContext();
        var repository = new EfMedicalStudyRepository(context);
        var dateTimeProvider = new FixedDateTimeProvider(new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc));

        var studyId = Guid.NewGuid();
        var study = MedicalStudy.Create(
            studyId,
            "existing-study.zip",
            "application/zip",
            2048 * 1024,
            "studies/existing.zip",
            dateTimeProvider).Value;

        await repository.Save(study);

        var retrievedStudy = await repository.GetById(studyId);

        retrievedStudy.Should().NotBeNull();
        var snapshot = retrievedStudy.ToSnapshot();
        snapshot.Id.Should().Be(studyId);
        snapshot.FileName.Should().Be("existing-study.zip");
        snapshot.ContentType.Should().Be("application/zip");
        snapshot.FileSizeBytes.Should().Be(2048 * 1024);
    }

    [Fact]
    public async Task GetById_WhenStudyDoesNotExist_ShouldReturnNull()
    {
        await using var context = _databaseFixture.CreateDbContext();
        var repository = new EfMedicalStudyRepository(context);

        var nonExistentId = Guid.NewGuid();

        var result = await repository.GetById(nonExistentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Save_WithExistingStudy_ShouldUpdateInDatabase()
    {
        await using var context = _databaseFixture.CreateDbContext();
        var repository = new EfMedicalStudyRepository(context);
        var dateTimeProvider = new FixedDateTimeProvider(new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc));

        var studyId = Guid.NewGuid();
        var originalStudy = MedicalStudy.Create(
            studyId,
            "original.zip",
            "application/zip",
            1024,
            "studies/original.zip",
            dateTimeProvider).Value;

        await repository.Save(originalStudy);

        var updatedDateTimeProvider = new FixedDateTimeProvider(new DateTime(2026, 1, 12, 11, 0, 0, DateTimeKind.Utc));
        var updatedStudy = MedicalStudy.Create(
            studyId,
            "updated.zip",
            "application/x-zip-compressed",
            2048,
            "studies/updated.zip",
            updatedDateTimeProvider).Value;

        await repository.Save(updatedStudy);

        var retrievedStudy = await repository.GetById(studyId);

        retrievedStudy.Should().NotBeNull();
        var snapshot = retrievedStudy.ToSnapshot();
        snapshot.FileName.Should().Be("updated.zip");
        snapshot.ContentType.Should().Be("application/x-zip-compressed");
        snapshot.FileSizeBytes.Should().Be(2048);
        snapshot.StorageKey.Should().Be("studies/updated.zip");
        snapshot.UploadDate.Should().Be(new DateTime(2026, 1, 12, 11, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task Save_WithMultipleStudies_ShouldPersistAll()
    {
        await using var context = _databaseFixture.CreateDbContext();
        var repository = new EfMedicalStudyRepository(context);
        var dateTimeProvider = new FixedDateTimeProvider(new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc));

        var study1 = MedicalStudy.Create(
            Guid.NewGuid(),
            "study1.zip",
            "application/zip",
            1024,
            "studies/1.zip",
            dateTimeProvider).Value;

        var study2 = MedicalStudy.Create(
            Guid.NewGuid(),
            "study2.zip",
            "application/zip",
            2048,
            "studies/2.zip",
            dateTimeProvider).Value;

        await repository.Save(study1);
        await repository.Save(study2);

        var retrievedStudy1 = await repository.GetById(study1.ToSnapshot().Id);
        var retrievedStudy2 = await repository.GetById(study2.ToSnapshot().Id);

        retrievedStudy1.Should().NotBeNull();
        retrievedStudy2.Should().NotBeNull();
        retrievedStudy1.ToSnapshot().FileName.Should().Be("study1.zip");
        retrievedStudy2.ToSnapshot().FileName.Should().Be("study2.zip");
    }

    [Fact]
    public async Task Save_WithSpecialCharactersInFileName_ShouldPersist()
    {
        await using var context = _databaseFixture.CreateDbContext();
        var repository = new EfMedicalStudyRepository(context);
        var dateTimeProvider = new FixedDateTimeProvider(new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc));

        var fileName = "étude-médicale-café-naïve.zip";
        var study = MedicalStudy.Create(
            Guid.NewGuid(),
            fileName,
            "application/zip",
            1024,
            "studies/special.zip",
            dateTimeProvider).Value;

        await repository.Save(study);

        var retrievedStudy = await repository.GetById(study.ToSnapshot().Id);

        retrievedStudy.Should().NotBeNull();
        retrievedStudy.ToSnapshot().FileName.Should().Be(fileName);
    }

    [Fact]
    public async Task Save_WithLargeFileSize_ShouldPersist()
    {
        await using var context = _databaseFixture.CreateDbContext();
        var repository = new EfMedicalStudyRepository(context);
        var dateTimeProvider = new FixedDateTimeProvider(new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc));

        var largeFileSize = 500L * 1024 * 1024; // 500 MB
        var study = MedicalStudy.Create(
            Guid.NewGuid(),
            "large-study.zip",
            "application/zip",
            largeFileSize,
            "studies/large.zip",
            dateTimeProvider).Value;

        await repository.Save(study);

        var retrievedStudy = await repository.GetById(study.ToSnapshot().Id);

        retrievedStudy.Should().NotBeNull();
        retrievedStudy.ToSnapshot().FileSizeBytes.Should().Be(largeFileSize);
    }

    [Fact]
    public async Task Save_WithCompleteExtractionLifecycle_ShouldPersistAllExtractionStates()
    {
        await using var context = _databaseFixture.CreateDbContext();
        var repository = new EfMedicalStudyRepository(context);
        var uploadDateTime = new DateTime(2026, 1, 13, 10, 0, 0, DateTimeKind.Utc);
        var startDateTime = new DateTime(2026, 1, 13, 10, 5, 0, DateTimeKind.Utc);
        var completedDateTime = new DateTime(2026, 1, 13, 10, 10, 0, DateTimeKind.Utc);

        var studyId = Guid.NewGuid();
        var study = MedicalStudy.Create(
            studyId,
            "ct-scan.zip",
            "application/zip",
            10 * 1024 * 1024,
            $"studies/{studyId}/ct-scan.zip",
            new FixedDateTimeProvider(uploadDateTime)).Value;

        await repository.Save(study);

        var uploadedStudy = await repository.GetById(studyId);
        uploadedStudy!.ToSnapshot().ExtractionStatus.Should().Be(ExtractionStatus.Pending);

        study.StartExtraction(new FixedDateTimeProvider(startDateTime));
        await repository.Save(study);

        var inProgressStudy = await repository.GetById(studyId);
        var inProgressSnapshot = inProgressStudy!.ToSnapshot();
        inProgressSnapshot.ExtractionStatus.Should().Be(ExtractionStatus.InProgress);
        inProgressSnapshot.ExtractionStartedAt.Should().Be(startDateTime);
        inProgressSnapshot.ExtractionCompletedAt.Should().BeNull();
        inProgressSnapshot.HuStatistics.Should().BeNull();

        var huStats = HuStatistics.Create(45.5, -1000.0, 3000.0, 125.3, 262144).Value;
        study.CompleteExtraction(huStats, new FixedDateTimeProvider(completedDateTime));
        await repository.Save(study);

        var completedStudy = await repository.GetById(studyId);
        var completedSnapshot = completedStudy!.ToSnapshot();
        completedSnapshot.ExtractionStatus.Should().Be(ExtractionStatus.Completed);
        completedSnapshot.ExtractionStartedAt.Should().Be(startDateTime);
        completedSnapshot.ExtractionCompletedAt.Should().Be(completedDateTime);
        completedSnapshot.HuStatistics.Should().NotBeNull();
        completedSnapshot.HuStatistics!.MeanHu.Should().Be(45.5);
        completedSnapshot.HuStatistics.MinHu.Should().Be(-1000.0);
        completedSnapshot.HuStatistics.MaxHu.Should().Be(3000.0);
        completedSnapshot.HuStatistics.StandardDeviation.Should().Be(125.3);
        completedSnapshot.HuStatistics.VoxelCount.Should().Be(262144);
        completedSnapshot.ExtractionError.Should().BeNull();
    }

    [Fact]
    public async Task Save_WithFailedExtraction_ShouldPersistErrorMessage()
    {
        await using var context = _databaseFixture.CreateDbContext();
        var repository = new EfMedicalStudyRepository(context);
        var uploadDateTime = new DateTime(2026, 1, 13, 10, 0, 0, DateTimeKind.Utc);
        var failedDateTime = new DateTime(2026, 1, 13, 10, 5, 0, DateTimeKind.Utc);

        var studyId = Guid.NewGuid();
        var study = MedicalStudy.Create(
            studyId,
            "invalid-dicom.zip",
            "application/zip",
            1024,
            $"studies/{studyId}/invalid.zip",
            new FixedDateTimeProvider(uploadDateTime)).Value;

        study.StartExtraction(new FixedDateTimeProvider(uploadDateTime));
        study.FailExtraction("Invalid DICOM file format", new FixedDateTimeProvider(failedDateTime));

        await repository.Save(study);

        var failedStudy = await repository.GetById(studyId);
        var failedSnapshot = failedStudy!.ToSnapshot();
        failedSnapshot.ExtractionStatus.Should().Be(ExtractionStatus.Failed);
        failedSnapshot.ExtractionCompletedAt.Should().Be(failedDateTime);
        failedSnapshot.HuStatistics.Should().BeNull();
        failedSnapshot.ExtractionError.Should().Be("Invalid DICOM file format");
    }

    private class FixedDateTimeProvider : IDateTimeProvider
    {
        private readonly DateTime _fixedDateTime;

        public FixedDateTimeProvider(DateTime fixedDateTime)
        {
            _fixedDateTime = fixedDateTime;
        }

        public DateTime Now => _fixedDateTime;
    }
}
