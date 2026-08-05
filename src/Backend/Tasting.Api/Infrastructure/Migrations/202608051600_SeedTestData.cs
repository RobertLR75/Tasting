using FluentMigrator;

namespace Tasting.Api.Infrastructure.Migrations;

[FluentMigrator.Tags("Development")]
[Migration(202608051600)]
public sealed class SeedTestData : Migration
{
    private static readonly Guid AdminUserId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid User1Id = new("00000000-0000-0000-0000-000000000002");
    private static readonly Guid User2Id = new("00000000-0000-0000-0000-000000000003");

    private static readonly Guid IpaStyleId = Guid.NewGuid();
    private static readonly Guid StoutStyleId = Guid.NewGuid();
    
    private static readonly Guid AleTypeId = Guid.NewGuid();
    
    private static readonly Guid LervigId = Guid.NewGuid();
    private static readonly Guid NogneId = Guid.NewGuid();
    
    private static readonly Guid Beer1Id = Guid.NewGuid();
    private static readonly Guid Beer2Id = Guid.NewGuid();
    private static readonly Guid Beer3Id = Guid.NewGuid();
    private static readonly Guid Beer4Id = Guid.NewGuid();

    private static readonly Guid FutureArrangementId = Guid.NewGuid();
    private static readonly Guid ActiveArrangementId = Guid.NewGuid();
    private static readonly Guid CompletedArrangementId = Guid.NewGuid();

    public override void Up()
    {
        // 1. Users
        // Admin already seeded in 202608041600_SeedAdminUser, but we add participants here
        Insert.IntoTable("users").Row(new
        {
            id = User1Id,
            email = "participant1@tasting.local",
            email_normalized = "participant1@tasting.local",
            first_name = "Ola",
            last_name = "Nordmann",
            password_hash = "$2a$11$v9L.1v7f185g9qH1X9FhOeY/2/HnJ/x913bL3e3h.Zz879/v4M6L2", // Test123!
            role = "User",
            is_active = true,
            created_at_utc = DateTimeOffset.UtcNow,
            updated_at_utc = (DateTimeOffset?)null
        });

        Insert.IntoTable("users").Row(new
        {
            id = User2Id,
            email = "participant2@tasting.local",
            email_normalized = "participant2@tasting.local",
            first_name = "Kari",
            last_name = "Nordmann",
            password_hash = "$2a$11$v9L.1v7f185g9qH1X9FhOeY/2/HnJ/x913bL3e3h.Zz879/v4M6L2", // Test123!
            role = "User",
            is_active = true,
            created_at_utc = DateTimeOffset.UtcNow,
            updated_at_utc = (DateTimeOffset?)null
        });

        // 2. Catalog - Beer Styles & Types
        Insert.IntoTable("beer_styles").Row(new { id = IpaStyleId, name = "IPA", created_at_utc = DateTimeOffset.UtcNow });
        Insert.IntoTable("beer_styles").Row(new { id = StoutStyleId, name = "Stout", created_at_utc = DateTimeOffset.UtcNow });
        
        Insert.IntoTable("beer_types").Row(new { id = AleTypeId, name = "Ale", created_at_utc = DateTimeOffset.UtcNow });

        // 3. Catalog - Breweries
        Insert.IntoTable("breweries").Row(new { id = LervigId, name = "Lervig", is_active = true, created_at_utc = DateTimeOffset.UtcNow });
        Insert.IntoTable("breweries").Row(new { id = NogneId, name = "Nøgne Ø", is_active = true, created_at_utc = DateTimeOffset.UtcNow });

        // 4. Catalog - Beers
        Insert.IntoTable("beers").Row(new { id = Beer1Id, name = "Tasty Juice", brewery_id = LervigId, beer_style_id = IpaStyleId, beer_type_id = AleTypeId, is_active = true, created_at_utc = DateTimeOffset.UtcNow });
        Insert.IntoTable("beers").Row(new { id = Beer2Id, name = "Supersonic", brewery_id = LervigId, beer_style_id = IpaStyleId, beer_type_id = AleTypeId, is_active = true, created_at_utc = DateTimeOffset.UtcNow });
        Insert.IntoTable("beers").Row(new { id = Beer3Id, name = "Global Pale Ale", brewery_id = NogneId, beer_style_id = IpaStyleId, beer_type_id = AleTypeId, is_active = true, created_at_utc = DateTimeOffset.UtcNow });
        Insert.IntoTable("beers").Row(new { id = Beer4Id, name = "Imperial Stout", brewery_id = NogneId, beer_style_id = StoutStyleId, beer_type_id = AleTypeId, is_active = true, created_at_utc = DateTimeOffset.UtcNow });

        // 5. Arrangements
        // Status enum mapping: 0=Created, 1=Started, 2=Canceled, 3=Completed
        
        Insert.IntoTable("arrangements").Row(new { 
            id = FutureArrangementId, 
            name = "Vinterølsmaking (Planlagt)", 
            description = "Fremtidig", 
            status = 0, // Created
            row_version = 1,
            created_at_utc = DateTimeOffset.UtcNow 
        });

        Insert.IntoTable("arrangements").Row(new { 
            id = ActiveArrangementId, 
            name = "IPA Bonanza (Pågår)", 
            description = "Aktiv smaking", 
            status = 1, // Started
            row_version = 2,
            created_at_utc = DateTimeOffset.UtcNow.AddDays(-1) 
        });

        Insert.IntoTable("arrangements").Row(new { 
            id = CompletedArrangementId, 
            name = "Sommersmaking", 
            description = "Historisk", 
            status = 3, // Completed
            row_version = 5,
            created_at_utc = DateTimeOffset.UtcNow.AddMonths(-3) 
        });

        // 6. Arrangement Participants
        Insert.IntoTable("arrangement_participants").Row(new { id = Guid.NewGuid(), arrangement_id = FutureArrangementId, user_id = User1Id, first_name_snapshot = "", last_name_snapshot = "", created_at_utc = DateTimeOffset.UtcNow });
        
        Insert.IntoTable("arrangement_participants").Row(new { id = Guid.NewGuid(), arrangement_id = ActiveArrangementId, user_id = User1Id, first_name_snapshot = "Ola", last_name_snapshot = "Nordmann", created_at_utc = DateTimeOffset.UtcNow.AddDays(-1) });
        Insert.IntoTable("arrangement_participants").Row(new { id = Guid.NewGuid(), arrangement_id = ActiveArrangementId, user_id = User2Id, first_name_snapshot = "Kari", last_name_snapshot = "Nordmann", created_at_utc = DateTimeOffset.UtcNow.AddDays(-1) });
        
        Insert.IntoTable("arrangement_participants").Row(new { id = Guid.NewGuid(), arrangement_id = CompletedArrangementId, user_id = User1Id, first_name_snapshot = "Ola", last_name_snapshot = "Nordmann", created_at_utc = DateTimeOffset.UtcNow.AddMonths(-3) });

        // 7. Arrangement Beers
        Insert.IntoTable("arrangement_beers").Row(new { id = Guid.NewGuid(), arrangement_id = FutureArrangementId, beer_id = Beer1Id, name_snapshot = "", brewery_name_snapshot = "", beer_style_snapshot = "", beer_type_snapshot = "", created_at_utc = DateTimeOffset.UtcNow });
        Insert.IntoTable("arrangement_beers").Row(new { id = Guid.NewGuid(), arrangement_id = FutureArrangementId, beer_id = Beer2Id, name_snapshot = "", brewery_name_snapshot = "", beer_style_snapshot = "", beer_type_snapshot = "", created_at_utc = DateTimeOffset.UtcNow });
        
        Insert.IntoTable("arrangement_beers").Row(new { id = Guid.NewGuid(), arrangement_id = ActiveArrangementId, beer_id = Beer1Id, name_snapshot = "Tasty Juice", brewery_name_snapshot = "Lervig", beer_style_snapshot = "IPA", beer_type_snapshot = "Ale", created_at_utc = DateTimeOffset.UtcNow.AddDays(-1) });
        Insert.IntoTable("arrangement_beers").Row(new { id = Guid.NewGuid(), arrangement_id = ActiveArrangementId, beer_id = Beer3Id, name_snapshot = "Global Pale Ale", brewery_name_snapshot = "Nøgne Ø", beer_style_snapshot = "IPA", beer_type_snapshot = "Ale", created_at_utc = DateTimeOffset.UtcNow.AddDays(-1) });

        Insert.IntoTable("arrangement_beers").Row(new { id = Guid.NewGuid(), arrangement_id = CompletedArrangementId, beer_id = Beer4Id, name_snapshot = "Imperial Stout", brewery_name_snapshot = "Nøgne Ø", beer_style_snapshot = "Stout", beer_type_snapshot = "Ale", created_at_utc = DateTimeOffset.UtcNow.AddMonths(-3) });
    }

    public override void Down()
    {
        // Deleting in reverse dependency order
        Delete.FromTable("arrangement_beers").AllRows();
        Delete.FromTable("arrangement_participants").AllRows();
        
        Delete.FromTable("arrangements")
            .Row(new { id = FutureArrangementId })
            .Row(new { id = ActiveArrangementId })
            .Row(new { id = CompletedArrangementId });

        Delete.FromTable("beers")
            .Row(new { id = Beer1Id })
            .Row(new { id = Beer2Id })
            .Row(new { id = Beer3Id })
            .Row(new { id = Beer4Id });

        Delete.FromTable("breweries")
            .Row(new { id = LervigId })
            .Row(new { id = NogneId });

        Delete.FromTable("beer_types").Row(new { id = AleTypeId });
        Delete.FromTable("beer_styles")
            .Row(new { id = IpaStyleId })
            .Row(new { id = StoutStyleId });

        Delete.FromTable("users")
            .Row(new { id = User1Id })
            .Row(new { id = User2Id });
    }
}
