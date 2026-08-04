var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

var tastingDb = postgres.AddDatabase("TastingDb", "tasting");

var tastingApi = builder.AddProject<Projects.Tasting_Api>("tasting-api")
    .WithReference(tastingDb)
    .WithHttpHealthCheck("/health")
    .WithUrlForEndpoint("http", endpoint =>
    {
        endpoint.DisplayText = "Scalar API Docs";
        endpoint.Url = $"{endpoint.Url.TrimEnd('/')}/scalar/v1";
    });

builder.AddProject<Projects.Tasting_Admin>("tasting-admin")
    .WithReference(tastingApi)
    .WithExternalHttpEndpoints();

builder.Build().Run();