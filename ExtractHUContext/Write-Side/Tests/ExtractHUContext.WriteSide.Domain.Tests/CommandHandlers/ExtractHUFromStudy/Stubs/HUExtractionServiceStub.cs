using ExtractHUContext.WriteSide.Domain.Models.ValueObjects;
using ExtractHUContext.WriteSide.Domain.Ports;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.Tests.CommandHandlers.ExtractHUFromStudy.Stubs;

public class HuExtractionServiceStub : IHuExtractionService
{
    private Result<HuStatistics>? _extractResult;

    public void SetExtractResult(Result<HuStatistics> result)
    {
        _extractResult = result;
    }

    public List<Stream> ExtractHuStatisticsCalls { get; } = [];

    public Task<Result<HuStatistics>> ExtractHuStatisticsAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        ExtractHuStatisticsCalls.Add(fileStream);

        if (_extractResult != null)
            return Task.FromResult(_extractResult);

        var defaultStats = HuStatistics.Create(
            meanHu: 50.0,
            minHu: -1000.0,
            maxHu: 1000.0,
            standardDeviation: 100.0,
            voxelCount: 1000000).Value;

        return Task.FromResult(Result.Success(defaultStats));
    }
}
