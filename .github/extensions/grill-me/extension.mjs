// Extension: grill-me
// C#/.NET Grill-me skill for Vertical Slice Architecture with FastEndpoints, MediatR, Aspire, Azure Key Vault, and Application Insights

import { joinSession } from "@github/copilot-sdk/extension";

// ─── Templates ──────────────────────────────────────────────────────────────

const fastEndpointsTemplate = (endpointName) => `
// Features/${endpointName}/${endpointName}Endpoint.cs
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Logging;

namespace YourApp.Features.${endpointName}
{
    public class ${endpointName}Request
    {
        public string Data { get; set; }
    }

    public class ${endpointName}Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    public class ${endpointName}Endpoint : Endpoint<${endpointName}Request, ${endpointName}Response>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<${endpointName}Endpoint> _logger;

        public ${endpointName}Endpoint(IMediator mediator, ILogger<${endpointName}Endpoint> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Post("/${endpointName.toLowerCase()}");
            AllowAnonymous();
            Description(b => b
                .WithName("${endpointName}")
                .WithSummary("Handle ${endpointName} operation")
                .WithDescription("Processes a ${endpointName} request and returns the result")
            );
        }

        public override async Task HandleAsync(${endpointName}Request req, CancellationToken ct)
        {
            _logger.LogInformation("Processing ${endpointName} request: {Data}", req.Data);

            try
            {
                var command = new ${endpointName}Command { Data = req.Data };
                var result = await _mediator.Send(command, ct);
                await SendOkAsync(result, cancellation: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ${endpointName}Endpoint");
                ThrowError((int)System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }

    public class ${endpointName}Command : IRequest<${endpointName}Response>
    {
        public string Data { get; set; }
    }
}
`;

const requestHandlerTemplate = (handlerName) => `
// Features/${handlerName}/${handlerName}Handler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using YourApp.Data;
using YourApp.Shared.Exceptions;

namespace YourApp.Features.${handlerName}
{
    public class ${handlerName}Request : IRequest<${handlerName}Response>
    {
        public string InputData { get; set; }
    }

    public class ${handlerName}Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    public class ${handlerName}Handler : IRequestHandler<${handlerName}Request, ${handlerName}Response>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<${handlerName}Handler> _logger;

        public ${handlerName}Handler(IApplicationDbContext dbContext, ILogger<${handlerName}Handler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<${handlerName}Response> Handle(${handlerName}Request request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting ${handlerName} with input: {InputData}", request.InputData);

            try
            {
                ValidateRequest(request);

                var result = await ExecuteBusinessLogicAsync(request, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("${handlerName} completed successfully");

                return new ${handlerName}Response { Success = true, Message = "Operation completed successfully", Data = result };
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error in ${handlerName}");
                return new ${handlerName}Response { Success = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ${handlerName}");
                throw;
            }
        }

        private void ValidateRequest(${handlerName}Request request)
        {
            if (string.IsNullOrWhiteSpace(request.InputData))
                throw new ValidationException("InputData cannot be empty");
        }

        private async Task<object> ExecuteBusinessLogicAsync(${handlerName}Request request, CancellationToken cancellationToken)
        {
            // TODO: Implement business logic here
            await Task.Delay(0, cancellationToken);
            return new { Message = "Business logic executed" };
        }
    }

    public static class ${handlerName}ServiceCollectionExtensions
    {
        public static IServiceCollection Add${handlerName}Feature(this IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<${handlerName}Request, ${handlerName}Response>, ${handlerName}Handler>();
            return services;
        }
    }
}
`;

const aspireKeyVaultTemplate = () => `
// AppHost/Program.cs — Aspire orchestration with Azure Key Vault
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var keyVault = builder.AddAzureKeyVault("keyvault");

var database = builder.AddSqlServer("sql").AddDatabase("appdb");

var api = builder
    .AddProject<Projects.YourApp_Api>("api")
    .WithReference(keyVault)
    .WithReference(database)
    .WithExternalHttpEndpoints();

var jobs = builder
    .AddProject<Projects.YourApp_Jobs>("jobs")
    .WithReference(keyVault)
    .WithReference(database);

await builder.Build().RunAsync();

// ---

// Api/Program.cs — Key Vault + FastEndpoints + MediatR wiring
using FastEndpoints;
using MediatR;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUrl = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_ENDPOINT");
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
    builder.Configuration.AddAzureKeyVault(
        secretClient,
        new Azure.Extensions.AspNetCore.Configuration.Secrets.KeyVaultSecretManager());
}

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddDbContext<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddFastEndpoints();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseFastEndpoints();
app.Run();
`;

const appInsightsTemplate = () => `
// Program.cs — Application Insights with MediatR behaviors
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using FastEndpoints;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer, CloudRoleNameTelemetryInitializer>();

builder.Services.AddFastEndpoints(config => { config.Endpoints.RoutePrefix = "api"; });
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
});

var app = builder.Build();
app.UseHttpsRedirection();
app.UseFastEndpoints();
app.Run();

// ---

// Shared/Telemetry/CloudRoleNameTelemetryInitializer.cs
public class CloudRoleNameTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "YourApp-Api";
        telemetry.Context.Cloud.RoleInstance = Environment.MachineName;
    }
}

// ---

// Shared/Behaviors/LoggingBehavior.cs
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var feature = typeof(TRequest).Namespace?.Split('.')[^1] ?? "Unknown";
        _logger.LogInformation("Starting feature: {Feature}", feature);
        var response = await next();
        _logger.LogInformation("Completed feature: {Feature}", feature);
        return response;
    }
}

// ---

// Shared/Behaviors/PerformanceBehavior.cs
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly IConfiguration _config;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var response = await next();
        timer.Stop();

        var threshold = _config.GetValue<int>("Logging:SlowQueryThresholdMs", 1000);
        if (timer.ElapsedMilliseconds > threshold)
        {
            _logger.LogWarning(
                "Slow request — Feature: {Feature}, Duration: {Duration}ms",
                typeof(TRequest).Namespace?.Split('.')[^1],
                timer.ElapsedMilliseconds);
        }

        return response;
    }
}
`;

const guidelinesTemplate = () => `
# C#/.NET Vertical Slice Architecture — Best Practices

## Core Principle
**One feature = one slice** (Request → Handler → Endpoint → Response)

\`\`\`
Features/
  CreateOrder/
    CreateOrderEndpoint.cs      # HTTP routing only — no business logic
    CreateOrderHandler.cs       # All business logic (IRequestHandler)
    CreateOrderRequest.cs       # Input DTO
    CreateOrderResponse.cs      # Output DTO
    CreateOrderValidator.cs     # FluentValidation rules
    CreateOrderServiceCollectionExtensions.cs
\`\`\`

## FastEndpoints
- Endpoint = HTTP adapter → delegates to MediatR immediately
- \`Configure()\` for routing, auth, Swagger
- **No business logic** in endpoints

\`\`\`csharp
public override void Configure()
{
    Post("/orders");
    Roles("Admin", "User");
}

public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
{
    var result = await _mediator.Send(new CreateOrderCommand { ... }, ct);
    await SendOkAsync(result, ct);
}
\`\`\`

## IRequestHandler
- **All** business logic lives here
- Validate → Execute → Persist → Return
- Inject: IApplicationDbContext, ILogger, IConfiguration

## Aspire + Azure Key Vault
\`\`\`csharp
var keyVault = builder.AddAzureKeyVault("keyvault");
var api = builder.AddProject<Projects.Api>("api").WithReference(keyVault);
\`\`\`
Secrets are auto-bound to \`IConfiguration\` in handlers.

## Application Insights
Register MediatR pipeline behaviors:
- \`LoggingBehavior\` — logs feature start/end
- \`PerformanceBehavior\` — alerts on slow requests

## Anti-Patterns
❌ Business logic in Endpoints  
❌ Feature-to-feature coupling  
❌ Shared service classes spanning features  
❌ Skipping validation in Handlers  

## New Feature Checklist
1. ✓ \`Features/MyFeature/\` folder
2. ✓ Request + Response DTOs
3. ✓ IRequestHandler implementation
4. ✓ FastEndpoints Endpoint
5. ✓ FluentValidation Validator
6. ✓ ServiceCollectionExtensions (DI)
7. ✓ Register in Program.cs
8. ✓ Unit tests for handler
`;

const structureGuideTemplate = () => `
# Vertical Slice Folder Structure

\`\`\`
src/
  Features/                             # One folder per vertical slice
    CreateOrder/
      CreateOrderEndpoint.cs            # FastEndpoints HTTP handler
      CreateOrderHandler.cs             # MediatR IRequestHandler (business logic)
      CreateOrderRequest.cs             # Input DTO + IRequest<TResponse>
      CreateOrderResponse.cs            # Output DTO
      CreateOrderValidator.cs           # FluentValidation rules
      CreateOrderServiceCollectionExtensions.cs
    GetOrderStatus/
      ...
    UpdateOrder/
      ...

  Data/
    ApplicationDbContext.cs             # Shared EF Core DbContext
    IApplicationDbContext.cs            # Interface for testability

  Shared/
    Behaviors/
      LoggingBehavior.cs                # MediatR pipeline: logging
      PerformanceBehavior.cs            # MediatR pipeline: perf tracking
    Exceptions/
      ValidationException.cs
      BusinessException.cs
    Telemetry/
      CloudRoleNameTelemetryInitializer.cs
    Extensions/
      ServiceCollectionExtensions.cs

AppHost/                                # Aspire orchestration project
  Program.cs

tests/
  Features/
    CreateOrder/
      CreateOrderHandlerTests.cs        # Unit tests per feature
\`\`\`

## Key Design Rules
- **Cohesion**: All code for a feature lives together
- **Independence**: Features don't call each other directly
- **Testability**: Handlers are testable without HTTP
- **Scalability**: Add features without touching existing slices
`;

const grillingSessionTemplate = () => `
# 🔥 Grill-Me Session — Sharpen Your Design

I will ask relentless, probing questions to expose gaps, risks, and weak assumptions in your plan or design.

## What I'll challenge you on:
- **Clarity** — Is the problem and solution clearly defined?
- **Completeness** — What edge cases or scenarios are missing?
- **Feasibility** — Is this technically achievable in your stack?
- **Risks** — What can go wrong? What are the failure modes?
- **Trade-offs** — Why this approach over alternatives?
- **Vertical slice fit** — Does this belong in one slice or multiple?
- **Testability** — How will you verify correctness?

## Ready?
Describe your plan, feature, or design — and I will grill you until it's bulletproof.
`;

// ─── Session ─────────────────────────────────────────────────────────────────

const session = await joinSession({
    tools: [
        {
            name: "grill-me_fastendpoints_template",
            description: "Generate a FastEndpoints endpoint template with IMediator integration",
            parameters: {
                type: "object",
                properties: {
                    endpointName: { type: "string", description: "Name of the endpoint (e.g., CreateUser, GetProduct)" }
                },
                required: ["endpointName"]
            },
            skipPermission: true,
            handler: async (args) => fastEndpointsTemplate(args.endpointName)
        },
        {
            name: "grill-me_requesthandler_template",
            description: "Generate an IRequestHandler implementation for MediatR pattern",
            parameters: {
                type: "object",
                properties: {
                    handlerName: { type: "string", description: "Name of the request handler" }
                },
                required: ["handlerName"]
            },
            skipPermission: true,
            handler: async (args) => requestHandlerTemplate(args.handlerName)
        },
        {
            name: "grill-me_aspire_keyvault_template",
            description: "Generate Aspire distributed application setup with Azure Key Vault integration",
            parameters: { type: "object", properties: {} },
            skipPermission: true,
            handler: async () => aspireKeyVaultTemplate()
        },
        {
            name: "grill-me_appinsights_template",
            description: "Generate Application Insights setup for FastEndpoints and MediatR",
            parameters: { type: "object", properties: {} },
            skipPermission: true,
            handler: async () => appInsightsTemplate()
        },
        {
            name: "grill-me_dotnet_guidelines",
            description: "Show best practices and guidelines for C#/.NET with FastEndpoints, Aspire, and Azure",
            parameters: { type: "object", properties: {} },
            skipPermission: true,
            handler: async () => guidelinesTemplate()
        },
        {
            name: "grill-me_dotnet_structure_guide",
            description: "Show recommended folder structure for vertical slice architecture",
            parameters: { type: "object", properties: {} },
            skipPermission: true,
            handler: async () => structureGuideTemplate()
        }
    ],
    hooks: {
        onSessionStart: async () => ({
            additionalContext:
                "C#/.NET Vertical Slice Architecture Grill-me Skill loaded!\n\n" +
                "🎯 Available Tools:\n" +
                "1. grill-me_fastendpoints_template - Generate FastEndpoints endpoint for a feature\n" +
                "2. grill-me_requesthandler_template - Generate IRequestHandler for MediatR pattern\n" +
                "3. grill-me_aspire_keyvault_template - Aspire setup with Azure Key Vault integration\n" +
                "4. grill-me_appinsights_template - Application Insights setup for vertical slices\n" +
                "5. grill-me_dotnet_guidelines - Best practices for vertical slice architecture\n" +
                "6. grill-me_dotnet_structure_guide - Recommended folder structure for vertical slices\n\n" +
                "💡 Slash Commands:\n" +
                "  /structure - Show vertical slice folder structure\n" +
                "  /guidelines - Show best practices and guidelines\n" +
                "  /grilling - Start a relentless grilling session to sharpen your plan/design\n\n" +
                "📁 Vertical Slice Pattern: Features/ folder with self-contained Request/Handler/Endpoint\n\n" +
                "Example usage:\n" +
                "  → /grilling\n" +
                "  → /structure\n" +
                "  → /guidelines\n" +
                "  → grill-me_fastendpoints_template CreateOrder\n" +
                "  → grill-me_requesthandler_template ProcessPayment\n" +
                "  → grill-me_aspire_keyvault_template\n" +
                "  → grill-me_appinsights_template"
        }),
        onUserPromptSubmitted: async (input) => {
            const prompt = input.prompt.trim();

            if (prompt === "/structure" || prompt.startsWith("/structure ")) {
                return {
                    modifiedPrompt: `Here's the vertical slice folder structure:\n\n${structureGuideTemplate()}`,
                    additionalContext: "Slash command /structure executed"
                };
            }

            if (prompt === "/guidelines" || prompt.startsWith("/guidelines ")) {
                return {
                    modifiedPrompt: `Here are the C#/.NET Vertical Slice Architecture best practices:\n\n${guidelinesTemplate()}`,
                    additionalContext: "Slash command /guidelines executed"
                };
            }

            if (prompt === "/grilling" || prompt.startsWith("/grilling ")) {
                return {
                    modifiedPrompt: grillingSessionTemplate() + "\n\nDescribe your plan, design, or idea — I'll start grilling you.",
                    additionalContext: "Slash command /grilling started — relentless questioning mode active"
                };
            }

            return undefined;
        }
    }
});
