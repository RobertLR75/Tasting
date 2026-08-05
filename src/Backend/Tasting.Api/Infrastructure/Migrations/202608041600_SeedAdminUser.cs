using FluentMigrator;

namespace Tasting.Api.Infrastructure.Migrations;

[Migration(202608041600)]
public sealed class SeedAdminUser : Migration
{
    private static readonly Guid UserId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    public override void Up()
    {
        Insert.IntoTable("users").Row(new
        {
            id = UserId,
            email = "robert.rodberget@gmail.com",
            email_normalized = "robert.rodberget@gmail.com",
            first_name = "Robert",
            last_name = "Lille-Rødberget",
            password_hash = "$2b$10$PI3SOfbseamFz67He1a3iufnBuh3RX.urM.Rd6zamUxwLgcRZNRlC",
            role = "Admin",
            is_active = true,
            created_at_utc = DateTimeOffset.UtcNow,
            updated_at_utc = (DateTimeOffset?)null
        });
    }

    public override void Down()
    {
        Delete.FromTable("users").Row(new { id = UserId });
    }
}
