using FluentMigrator;

namespace Tasting.Api.Infrastructure.Migrations;

[Migration(202502200100)]
public sealed class AddPasswordHashToUsers : Migration
{
    public override void Up()
    {
        Alter.Table("users")
            .AddColumn("password_hash").AsString(255).Nullable();
    }

    public override void Down()
    {
        Delete.Column("password_hash").FromTable("users");
    }
}
