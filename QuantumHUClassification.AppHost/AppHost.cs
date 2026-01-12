var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("quantumhu");

var api = builder.AddProject<Projects.QuantumHUClassification_Api>("api")
    .WithReference(postgres);

builder.Build().Run();
