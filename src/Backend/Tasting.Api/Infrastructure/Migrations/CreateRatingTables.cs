using FluentMigrator;

namespace Tasting.Api.Infrastructure.Migrations;

[Migration(20250101000001)]
public class CreateRatingTables : Migration
{
    public override void Up()
    {
        Create.Table("ratings")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("arrangement_id").AsGuid().NotNullable()
            .WithColumn("participant_id").AsGuid().NotNullable()
            .WithColumn("beer_id").AsGuid().NotNullable()
            .WithColumn("visibility").AsDecimal(4, 1).NotNullable()
            .WithColumn("smell").AsDecimal(4, 1).NotNullable()
            .WithColumn("taste").AsDecimal(4, 1).NotNullable()
            .WithColumn("toast").AsDecimal(4, 1).NotNullable()
            .WithColumn("total_rating").AsDecimal(5, 2).NotNullable()
            .WithColumn("row_version").AsInt64().NotNullable().WithDefaultValue(1)
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at_utc").AsDateTimeOffset().Nullable();

        Create.UniqueConstraint("uq_ratings_arrangement_participant_beer")
            .OnTable("ratings")
            .Columns("arrangement_id", "participant_id", "beer_id");

        Create.Table("results")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("arrangement_id").AsGuid().NotNullable()
            .WithColumn("beer_id").AsGuid().NotNullable()
            .WithColumn("beer_name_snapshot").AsString(200).NotNullable()
            .WithColumn("total_rating").AsDecimal(5, 2).NotNullable()
            .WithColumn("rating_count").AsInt32().NotNullable()
            .WithColumn("standard_deviation").AsDecimal(10, 6).NotNullable()
            .WithColumn("rank").AsInt32().NotNullable()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at_utc").AsDateTimeOffset().Nullable();

        Create.UniqueConstraint("uq_results_arrangement_beer")
            .OnTable("results")
            .Columns("arrangement_id", "beer_id");

        Create.Table("result_participants")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("result_id").AsGuid().NotNullable()
            .WithColumn("participant_id").AsGuid().NotNullable()
            .WithColumn("participant_name_snapshot").AsString(200).NotNullable()
            .WithColumn("rating").AsDecimal(5, 2).NotNullable();

        Create.UniqueConstraint("uq_result_participants_result_participant")
            .OnTable("result_participants")
            .Columns("result_id", "participant_id");

        Create.ForeignKey("fk_result_participants_result_id")
            .FromTable("result_participants").ForeignColumn("result_id")
            .ToTable("results").PrimaryColumn("id");
    }

    public override void Down()
    {
        Delete.Table("result_participants");
        Delete.Table("results");
        Delete.Table("ratings");
    }
}
