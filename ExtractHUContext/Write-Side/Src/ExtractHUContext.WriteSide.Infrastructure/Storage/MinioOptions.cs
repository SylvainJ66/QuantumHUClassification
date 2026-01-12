namespace ExtractHUContext.WriteSide.Infrastructure.Storage;

public class MinioOptions
{
    public const string SectionName = "Minio";

    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "medical-studies";
    public bool UseSSL { get; set; } = false;
}
