using ExtractHUContext.ReadSide.Domain.Ports;
using ExtractHUContext.ReadSide.Domain.QueryHandlers.GetAllQuantumGreetings;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Wolverine;
using SharedKernel.Domain;
using SharedKernel.Infrastructure;
using ExtractHUContext.WriteSide.Infrastructure.Persistence;
using ExtractHUContext.WriteSide.Infrastructure.Persistence.Repositories;
using ExtractHUContext.ReadSide.Infrastructure.Queries;
using ExtractHUContext.WriteSide.Domain.CommandHandlers.CreateQuantumGreeting;
using ExtractHUContext.WriteSide.Domain.Ports;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Quantum HU Classification API",
            Version = "v1",
            Description = "API for managing Quantum Greetings using CQRS pattern with Wolverine message bus"
        };
        return Task.CompletedTask;
    });
});

// Connection string is provided by:
// 1. Aspire (via environment variables in orchestrated mode)
// 2. User Secrets (in development when running standalone)
// 3. Environment variables (in production)
var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException(
        "Database connection string not found. " +
        "For local development, configure User Secrets: dotnet user-secrets set \"ConnectionStrings:Database\" \"your-connection-string\"");

// Write-Side Infrastructure
builder.Services.AddDbContext<QuantumHUDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IQuantumGreetingRepository, EfQuantumGreetingRepository>();

// Read-Side Infrastructure
builder.Services.AddSingleton<IDbConnectionFactory>(sp =>
    new NpgsqlConnectionFactory(connectionString));

builder.Services.AddScoped<IGetAllQuantumGreetingsQuery, SqlGetAllQuantumGreetingsQuery>();

// SharedKernel services
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

// Configure Wolverine
builder.Host.UseWolverine(opts =>
{
    // Disable automatic assembly scanning to prevent issues with build-time assemblies
    opts.Discovery.DisableConventionalDiscovery();

    opts.Discovery.IncludeAssembly(typeof(CreateQuantumGreetingHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(GetAllQuantumGreetingsHandler).Assembly);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Quantum HU Classification API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.MapControllers();

// Apply migrations on startup in development
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<QuantumHUDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
