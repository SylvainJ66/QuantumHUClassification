var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("quantumhu");

var api = builder.AddProject<Projects.QuantumHUContext_Api>("api")
    .WithReference(postgres);

builder.Build().Run();
