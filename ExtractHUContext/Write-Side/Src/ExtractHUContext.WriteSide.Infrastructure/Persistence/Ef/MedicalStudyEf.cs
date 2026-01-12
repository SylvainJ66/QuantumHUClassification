namespace ExtractHUContext.WriteSide.Infrastructure.Persistence.Ef;

public class MedicalStudyEf
{
    public Guid Id { get; set; }
    public DateTime UploadDate { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StorageKey { get; set; } = string.Empty;
}
