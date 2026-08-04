using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users.Login;

namespace Tasting.Api.Features.Identity.Users.Login;

public sealed class LoginEndpoint(IRequestHandler<LoginCommand, LoginResponse> handler) : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/users/login");
        AllowAnonymous();
        Description(d => d.WithTags("Auth"));
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var command = new LoginCommand(req.Email, req.Password);
        var response = await handler.HandleAsync(command, ct);
        await Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
    }
}
