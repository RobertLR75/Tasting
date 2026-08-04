using FluentMigrator;

namespace Tasting.Api.Infrastructure.Migrations;

[Migration(202608031701)]
public sealed class CreateUsersTable : Migration
{
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("email").AsString(200).NotNullable()
            .WithColumn("email_normalized").AsString(200).NotNullable()
            .WithColumn("first_name").AsString(200).NotNullable()
            .WithColumn("last_name").AsString(200).NotNullable()
            .WithColumn("is_active").AsBoolean().NotNullable()
            .WithColumn("role").AsString(20).NotNullable()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at_utc").AsDateTimeOffset().Nullable();

        Create.Index("ix_users_email_normalized")
            .OnTable("users")
            .OnColumn("email_normalized").Ascending()
            .WithOptions().Unique();

        Execute.Sql("ALTER TABLE users ADD CONSTRAINT ck_users_role CHECK (role IN ('Admin', 'User'));");
    }

    public override void Down()
    {
        Delete.Table("users");
    }
}
