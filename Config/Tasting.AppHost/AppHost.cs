var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

var tastingDb = postgres.AddDatabase("TastingDb", "tasting");

var tastingApi = builder.AddProject<Projects.Tasting_Api>("tasting-api")
    .WithReference(tastingDb)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Tasting_Admin>("tasting-admin")
    .WithReference(tastingApi)
    .WithExternalHttpEndpoints();

builder.Build().Run();