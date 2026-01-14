using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.Models.ValueObjects;

public class HuStatistics
{
    public double MeanHu { get; }
    public double MinHu { get; }
    public double MaxHu { get; }
    public double StandardDeviation { get; }
    public long VoxelCount { get; }

    private HuStatistics(
        double meanHu,
        double minHu,
        double maxHu,
        double standardDeviation,
        long voxelCount)
    {
        MeanHu = meanHu;
        MinHu = minHu;
        MaxHu = maxHu;
        StandardDeviation = standardDeviation;
        VoxelCount = voxelCount;
    }

    public static Result<HuStatistics> Create(
        double meanHu,
        double minHu,
        double maxHu,
        double standardDeviation,
        long voxelCount)
    {
        if (minHu > maxHu)
            return Result.Failure<HuStatistics>("Minimum HU cannot be greater than maximum HU");

        if (meanHu < minHu || meanHu > maxHu)
            return Result.Failure<HuStatistics>("Mean HU must be between minimum and maximum HU");

        if (standardDeviation < 0)
            return Result.Failure<HuStatistics>("Standard deviation cannot be negative");

        if (voxelCount <= 0)
            return Result.Failure<HuStatistics>("Voxel count must be greater than 0");

        return Result.Success(new HuStatistics(meanHu, minHu, maxHu, standardDeviation, voxelCount));
    }
}
