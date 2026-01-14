namespace ExtractHUContext.WriteSide.Infrastructure.Persistence.Ef;

public class MedicalStudyEf
{
    public Guid Id { get; init; }
    public DateTime UploadDate { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public int ExtractionStatus { get; set; }
    public DateTime? ExtractionStartedAt { get; set; }
    public DateTime? ExtractionCompletedAt { get; set; }
    public double? MeanHu { get; set; }
    public double? MinHu { get; set; }
    public double? MaxHu { get; set; }
    public double? StandardDeviation { get; set; }
    public long? VoxelCount { get; set; }
    public string? ExtractionError { get; set; }
}
