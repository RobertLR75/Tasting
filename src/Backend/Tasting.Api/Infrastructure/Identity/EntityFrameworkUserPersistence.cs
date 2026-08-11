using SharedLibrary.PostgreSql.EntityFramework;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Infrastructure.Identity;

public sealed class EntityFrameworkUserPersistence(UsersDbContext context)
    : EntityFrameworkPostgresSqlStorageBase<User>(context);
