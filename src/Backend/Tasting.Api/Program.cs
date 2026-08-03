using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Configuration;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Configuration;
using Tasting.Api.Infrastructure.Catalog;
using Tasting.Api.Infrastructure.Identity;
using Tasting.Api.Infrastructure.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.ConfigureFastEndPoints();
builder.ConfigureServices();
builder.Services.AddIdentityInfrastructure(builder.Configuration);

var oidcSettings = builder.Configuration
    .GetSection("OpenIdConnect")
    .Get<OpenIdConnectSettings>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = oidcSettings?.Authority?.ToString();
        options.Audience = oidcSettings?.ClientId;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());
builder.Services.AddCatalog(builder.Configuration);

var app = builder.Build();

var connectionString = app.Configuration.GetConnectionString("TastingDb");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    new TastingMigrationService().MigrateUp(connectionString);
}

app.UseAuthentication();
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();
app.UseEndpoints(routePrefix: "api/v1");

app.MapDefaultEndpoints();

app.Run();

public partial class Program;
