var builder = DistributedApplication.CreateBuilder(args);

var tastingApi = builder.AddProject<Projects.Tasting_Api>("tasting-api")
    .WithHttpHealthCheck("/health");

builder.Build().Run();