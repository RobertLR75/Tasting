using FluentMigrator;

namespace Tasting.Api.Infrastructure.Migrations;

[Migration(202608041500)]
public sealed class AddPasswordHashToUsers : Migration
{
    public override void Up()
    {
        if (Schema.Table("users").Column("password_hash").Exists())
        {
            return;
        }

        Alter.Table("users")
            .AddColumn("password_hash").AsString(255).Nullable();
    }

    public override void Down()
    {
        if (!Schema.Table("users").Column("password_hash").Exists())
        {
            return;
        }

        Delete.Column("password_hash").FromTable("users");
    }
}
