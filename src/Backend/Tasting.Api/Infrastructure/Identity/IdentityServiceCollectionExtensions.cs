using System.Data.Common;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedLibrary.Configuration;
using SharedLibrary.Interfaces;
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
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        PersistenceConfiguration persistence)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(persistence);
        // Arrangement still reads UsersDbContext until its own persistence ticket is implemented.
        // Identity itself is always resolved through the globally selected persistence service below.
        services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(persistence.ConnectionString));

        if (persistence.Provider == PersistenceProvider.EntityFramework)
        {
            services.AddScoped<IPersistenceService<User>, EntityFrameworkUserPersistence>();
        }
        else
        {
            SqlMapper.AddTypeHandler(new UserRoleTypeHandler());
            services.AddScoped<DbConnection>(_ => new NpgsqlConnection(persistence.ConnectionString));
            services.AddScoped<IPersistenceService<User>, DapperUserPersistence>();
        }

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
