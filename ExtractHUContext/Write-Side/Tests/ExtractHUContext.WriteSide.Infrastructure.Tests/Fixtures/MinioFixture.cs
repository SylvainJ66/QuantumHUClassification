using Minio;
using Testcontainers.Minio;

namespace ExtractHUContext.WriteSide.Infrastructure.Tests.Fixtures;

public class MinioFixture : IAsyncLifetime
{
    private readonly MinioContainer _container;

    public MinioFixture()
    {
        _container = new MinioBuilder("minio/minio:latest")
            .Build();
    }

    public string GetEndpoint()
    {
        var connectionString = _container.GetConnectionString();
        return connectionString.Replace("http://", "").Replace("https://", "").TrimEnd('/');
    }

    public string AccessKey => _container.GetAccessKey();
    public string SecretKey => _container.GetSecretKey();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public IMinioClient CreateMinioClient()
    {
        return new MinioClient()
            .WithEndpoint(GetEndpoint())
            .WithCredentials(AccessKey, SecretKey)
            .WithSSL(false)
            .Build();
    }
}
