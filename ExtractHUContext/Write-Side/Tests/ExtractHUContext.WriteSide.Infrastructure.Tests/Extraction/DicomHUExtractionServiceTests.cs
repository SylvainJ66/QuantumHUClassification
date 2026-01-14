using ExtractHUContext.WriteSide.Infrastructure.Extraction;
using ExtractHUContext.WriteSide.Infrastructure.Tests.Helpers;
using FluentAssertions;

namespace ExtractHUContext.WriteSide.Infrastructure.Tests.Extraction;

public class DicomHuExtractionServiceTests
{
    private readonly DicomHuExtractionService _service = new();

    [Fact]
    public async Task ExtractHUStatisticsAsync_WithSingleCTDicomFile_ShouldExtractValidStatistics()
    {
        await using var dicomStream = TestDicomFileBuilder.CreateSimpleCtDicomStream(
            width: 512,
            height: 512,
            rescaleSlope: 1.0,
            rescaleIntercept: -1024.0);

        var result = await _service.ExtractHuStatisticsAsync(dicomStream);

        result.IsSuccess.Should().BeTrue();
        var stats = result.Value;
        stats.VoxelCount.Should().Be(512 * 512);
        stats.MeanHu.Should().BeApproximately(350.0, 100.0);
        stats.MinHu.Should().BeInRange(-1024.0, 50.0);
        stats.MaxHu.Should().BeGreaterThan(0);
        stats.StandardDeviation.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExtractHUStatisticsAsync_WithZipContainingMultipleDicomFiles_ShouldAggregateAllFiles()
    {
        await using var zipStream = TestDicomFileBuilder.CreateZipWithMultipleDicomFiles(fileCount: 3);

        var result = await _service.ExtractHuStatisticsAsync(zipStream);

        result.IsSuccess.Should().BeTrue();
        var stats = result.Value;
        stats.VoxelCount.Should().Be(3 * 256 * 256);
        stats.MeanHu.Should().BeApproximately(350.0, 100.0);
        stats.MinHu.Should().BeInRange(-1024.0, 50.0);
        stats.MaxHu.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExtractHUStatisticsAsync_WithNonCTDicomFile_ShouldReturnFailure()
    {
        await using var mrStream = TestDicomFileBuilder.CreateNonCtDicomStream();

        var result = await _service.ExtractHuStatisticsAsync(mrStream);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No valid DICOM files with pixel data found");
    }

    [Fact]
    public async Task ExtractHUStatisticsAsync_WithEmptyStream_ShouldReturnFailure()
    {
        await using var emptyStream = new MemoryStream();

        var result = await _service.ExtractHuStatisticsAsync(emptyStream);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("File stream is empty");
    }

    [Fact]
    public async Task ExtractHUStatisticsAsync_WithNullStream_ShouldReturnFailure()
    {
        var result = await _service.ExtractHuStatisticsAsync(null!);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid file stream");
    }

    [Fact]
    public async Task ExtractHUStatisticsAsync_WithValidHURange_ShouldCalculateCorrectStatistics()
    {
        await using var dicomStream = TestDicomFileBuilder.CreateSimpleCtDicomStream(
            width: 256,
            height: 256,
            rescaleSlope: 1.0,
            rescaleIntercept: -1024.0);

        var result = await _service.ExtractHuStatisticsAsync(dicomStream);

        result.IsSuccess.Should().BeTrue();
        var stats = result.Value;
        stats.MinHu.Should().BeLessThanOrEqualTo(stats.MeanHu);
        stats.MeanHu.Should().BeLessThanOrEqualTo(stats.MaxHu);
        stats.StandardDeviation.Should().BeGreaterThanOrEqualTo(0);
        stats.VoxelCount.Should().BeGreaterThan(0);
    }
}
