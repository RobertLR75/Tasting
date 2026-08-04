var builder = DistributedApplication.CreateBuilder(args);

var tastingApi = builder.AddProject<Projects.Tasting_Api>("tasting-api")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Tasting_Admin>("tasting-admin")
    .WithReference(tastingApi)
    .WithExternalHttpEndpoints();

builder.Build().Run();