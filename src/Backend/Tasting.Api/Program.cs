using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SharedLibrary.Configuration;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Configuration;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Catalog;
using Tasting.Api.Infrastructure.Identity;
using Tasting.Api.Infrastructure.Migrations;
using Tasting.Api.Infrastructure.Rating;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.ConfigureFastEndPoints();
builder.ConfigureServices();
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.AddRatingServices();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not configured");
var key = System.Text.Encoding.UTF8.GetBytes(secretKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            NameClaimType = "email",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddCatalog(builder.Configuration);
builder.Services.AddArrangement(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(FastEndPointsExtensions.WriteErrorResponseAsync));

var connectionString = app.Configuration.GetConnectionString("TastingDb");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    var tags = app.Environment.IsDevelopment() ? new[] { "Development" } : Array.Empty<string>();
    new TastingMigrationService().MigrateUp(connectionString, tags);
}

app.UseCors();
app.UseAuthentication();
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();
app.UseEndpoints(routePrefix: "api/v1");

app.MapScalarApiReference("/scalar/v1").AllowAnonymous();

app.MapDefaultEndpoints();

app.Run();

public partial class Program;
