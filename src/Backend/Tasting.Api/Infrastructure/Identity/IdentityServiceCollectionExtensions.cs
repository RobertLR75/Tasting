using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Features.Identity.Users.CreateUser;
using Tasting.Api.Features.Identity.Users.DeactivateUser;
using Tasting.Api.Features.Identity.Users.GetUser;
using Tasting.Api.Features.Identity.Users.ListUsers;
using Tasting.Api.Features.Identity.Users.Login;
using Tasting.Api.Features.Identity.Users.UpdateUser;
using Tasting.Api.Infrastructure.Security;

namespace Tasting.Api.Infrastructure.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("TastingDb");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(connectionString));
        }
        else
        {
            services.AddDbContext<UsersDbContext>(options => options.UseInMemoryDatabase("tasting-api"));
        }

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IRequestHandler<CreateUserCommand, User>, CreateUserHandler>();
        services.AddScoped<IRequestHandler<GetUserQuery, User>, GetUserHandler>();
        services.AddScoped<IRequestHandler<DeactivateUserCommand, User>, DeactivateUserHandler>();
        services.AddScoped<IRequestHandler<ListUsersQuery, ListUsersResult>, ListUsersHandler>();
        services.AddScoped<IRequestHandler<UpdateUserCommand, User>, UpdateUserHandler>();
        services.AddScoped<IRequestHandler<LoginCommand, LoginResponse>, LoginHandler>();

        return services;
    }
}
