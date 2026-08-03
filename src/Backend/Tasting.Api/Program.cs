using Microsoft.AspNetCore.Authentication.JwtBearer;
using SharedLibrary.Configuration;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Configuration;
using Tasting.Api.Infrastructure.Migrations;
using Tasting.Api.Infrastructure.Rating;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.ConfigureFastEndPoints();
builder.ConfigureServices();
builder.AddRatingServices();

var oidcSettings = builder.Configuration
    .GetSection("OpenIdConnect")
    .Get<OpenIdConnectSettings>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = oidcSettings?.Authority?.ToString();
        options.Audience = oidcSettings?.ClientId;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

var connectionString = app.Configuration.GetConnectionString("TastingDb");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    new TastingMigrationService().MigrateUp(connectionString);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(routePrefix: "api/v1");

app.MapDefaultEndpoints();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
