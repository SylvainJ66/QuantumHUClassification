using ExtractHUContext.WriteSide.Domain.Models.ValueObjects;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.Ports;

public interface IHuExtractionService
{
    Task<Result<HuStatistics>> ExtractHuStatisticsAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
