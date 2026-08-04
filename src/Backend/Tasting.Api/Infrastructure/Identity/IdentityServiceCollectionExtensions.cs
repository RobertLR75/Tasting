using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Features.Identity.Users.CreateUser;
using Tasting.Api.Features.Identity.Users.DeactivateUser;
using Tasting.Api.Features.Identity.Users.GetUser;

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

        services.AddScoped<IRequestHandler<CreateUserCommand, User>, CreateUserHandler>();
        services.AddScoped<IRequestHandler<GetUserQuery, User>, GetUserHandler>();
        services.AddScoped<IRequestHandler<DeactivateUserCommand, User>, DeactivateUserHandler>();

        return services;
    }
}
