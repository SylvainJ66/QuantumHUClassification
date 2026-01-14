using ExtractHUContext.ReadSide.Domain.Ports;
using ExtractHUContext.ReadSide.Domain.QueryHandlers.GetAllMedicalStudies;
using ExtractHUContext.WriteSide.Domain.CommandHandlers.ExtractHUFromStudy;
using ExtractHUContext.WriteSide.Domain.Ports;
using ExtractHUContext.WriteSide.Infrastructure.Extraction;
using ExtractHUContext.WriteSide.Infrastructure.Persistence;
using ExtractHUContext.WriteSide.Infrastructure.Persistence.Repositories;
using ExtractHUContext.WriteSide.Infrastructure.Storage;
using ExtractHUContext.ReadSide.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Minio;
using Scalar.AspNetCore;
using SharedKernel.Domain;
using SharedKernel.Infrastructure;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Quantum HU Classification API",
            Version = "v1",
            Description = "API for managing Medical Studies using CQRS pattern with Wolverine message bus"
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

builder.Services.AddScoped<IMedicalStudyRepository, EfMedicalStudyRepository>();

// Read-Side Infrastructure
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new NpgsqlConnectionFactory(connectionString));

builder.Services.AddScoped<IGetAllMedicalStudiesQuery, SqlGetAllMedicalStudiesQuery>();

// SharedKernel services
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

// MinIO Configuration
builder.Services.Configure<MinioOptions>(
    builder.Configuration.GetSection(MinioOptions.SectionName));

var minioOptions = builder.Configuration
    .GetSection(MinioOptions.SectionName)
    .Get<MinioOptions>()
    ?? throw new InvalidOperationException("MinIO configuration not found");

if (string.IsNullOrEmpty(minioOptions.AccessKey) || string.IsNullOrEmpty(minioOptions.SecretKey))
    throw new InvalidOperationException("MinIO AccessKey and SecretKey must be configured in user secrets");

builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(minioOptions.Endpoint)
    .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
    .WithSSL(minioOptions.UseSSL));

builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();
builder.Services.AddScoped<IHuExtractionService, DicomHuExtractionService>();

// Configure Wolverine
builder.Host.UseWolverine(opts =>
{
    // Include assemblies for Wolverine handler discovery
    opts.Discovery.IncludeAssembly(typeof(GetAllMedicalStudiesHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(ExtractHuFromStudyHandler).Assembly);
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
