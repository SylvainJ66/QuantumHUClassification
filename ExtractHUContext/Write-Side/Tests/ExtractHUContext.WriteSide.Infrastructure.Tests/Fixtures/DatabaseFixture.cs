using ExtractHUContext.WriteSide.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace ExtractHUContext.WriteSide.Infrastructure.Tests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public DatabaseFixture()
    {
        _container = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public QuantumHUDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<QuantumHUDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        var context = new QuantumHUDbContext(options);
        context.Database.Migrate();
        return context;
    }
}
