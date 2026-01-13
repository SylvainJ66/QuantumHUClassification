using ExtractHUContext.ReadSide.Domain.QueryHandlers.GetAllMedicalStudies;
using ExtractHUContext.ReadSide.Domain.QueryHandlers.GetAllMedicalStudies.Queries;
using ExtractHUContext.ReadSide.Domain.ReadModels;
using ExtractHUContext.ReadSide.Domain.Tests.QueryHandlers.GetAllMedicalStudies.Stubs;
using FluentAssertions;

namespace ExtractHUContext.ReadSide.Domain.Tests.QueryHandlers.GetAllMedicalStudies;

public class GetAllMedicalStudiesHandlerTests
{
    private readonly GetAllMedicalStudiesQueryStub _queryStub;
    private readonly GetAllMedicalStudiesQuery _query;

    public GetAllMedicalStudiesHandlerTests()
    {
        _queryStub = new GetAllMedicalStudiesQueryStub();
        _query = new GetAllMedicalStudiesQuery();
    }

    [Fact]
    public async Task Handle_WithNoStudies_ShouldReturnEmptyList()
    {
        var result = await GetAllMedicalStudiesHandler.Handle(_query, _queryStub);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithOneStudy_ShouldReturnCorrectStudy()
    {
        var studyId = Guid.NewGuid();
        var uploadDate = new DateTime(2026, 1, 12, 10, 30, 0, DateTimeKind.Utc);
        var study = new MedicalStudyReadModel(
            Id: studyId,
            UploadDate: uploadDate,
            FileName: "MRI_Brain.zip",
            ContentType: "application/zip",
            FileSizeBytes: 10485760, // 10 MB
            StorageKey: $"studies/{studyId}/{studyId}.zip"
        );
        _queryStub.AddStudy(study);

        var result = await GetAllMedicalStudiesHandler.Handle(_query, _queryStub);

        result.Should().HaveCount(1);
        var returnedStudy = result.First();
        returnedStudy.Id.Should().Be(studyId);
        returnedStudy.UploadDate.Should().Be(uploadDate);
        returnedStudy.FileName.Should().Be("MRI_Brain.zip");
        returnedStudy.ContentType.Should().Be("application/zip");
        returnedStudy.FileSizeBytes.Should().Be(10485760);
        returnedStudy.StorageKey.Should().Be($"studies/{studyId}/{studyId}.zip");
    }

    [Fact]
    public async Task Handle_WithMultipleStudies_ShouldReturnAllOrderedByDateDescending()
    {
        var oldestDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var middleDate = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc);
        var newestDate = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        var oldStudy = CreateMedicalStudyReadModel(fileName: "Old.zip", uploadDate: oldestDate, fileSizeBytes: 1024);
        var middleStudy = CreateMedicalStudyReadModel(fileName: "Middle.zip", uploadDate: middleDate, contentType: "application/x-compressed");
        var newStudy = CreateMedicalStudyReadModel(fileName: "New.zip", uploadDate: newestDate, fileSizeBytes: 500 * 1024 * 1024);

        // Add in random order to verify sorting
        _queryStub.AddStudies(new[] { middleStudy, oldStudy, newStudy });

        var result = await GetAllMedicalStudiesHandler.Handle(_query, _queryStub);

        // Verify all studies returned
        result.Should().HaveCount(3);
        result.Should().Contain(oldStudy);
        result.Should().Contain(middleStudy);
        result.Should().Contain(newStudy);

        // Verify ordered by date descending (newest first)
        var resultList = result.ToList();
        resultList[0].Should().BeEquivalentTo(newStudy);
        resultList[1].Should().BeEquivalentTo(middleStudy);
        resultList[2].Should().BeEquivalentTo(oldStudy);

        // Verify different content types and file sizes are handled
        resultList.Select(s => s.ContentType).Should().Contain("application/zip");
        resultList.Select(s => s.ContentType).Should().Contain("application/x-compressed");
        resultList.Select(s => s.FileSizeBytes).Should().Contain(1024);
        resultList.Select(s => s.FileSizeBytes).Should().Contain(500 * 1024 * 1024);
    }

    private static MedicalStudyReadModel CreateMedicalStudyReadModel(
        Guid? id = null,
        DateTime? uploadDate = null,
        string fileName = "test-study.zip",
        string contentType = "application/zip",
        long fileSizeBytes = 1048576) // 1 MB default
    {
        var studyId = id ?? Guid.NewGuid();
        return new MedicalStudyReadModel(
            Id: studyId,
            UploadDate: uploadDate ?? DateTime.UtcNow,
            FileName: fileName,
            ContentType: contentType,
            FileSizeBytes: fileSizeBytes,
            StorageKey: $"studies/{studyId}/{studyId}.zip"
        );
    }
}
