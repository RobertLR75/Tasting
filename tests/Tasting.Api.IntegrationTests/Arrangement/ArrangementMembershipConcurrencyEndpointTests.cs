using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Catalog;
using Xunit;

namespace Tasting.Api.IntegrationTests.Arrangement;

public sealed class ArrangementMembershipConcurrencyEndpointTests(ArrangementApiFactory factory)
    : IClassFixture<ArrangementApiFactory>
{
    [Theory]
    [InlineData(MembershipMutation.AddBeer)]
    [InlineData(MembershipMutation.RemoveBeer)]
    [InlineData(MembershipMutation.AddParticipant)]
    [InlineData(MembershipMutation.RemoveParticipant)]
    public async Task MembershipEndpoint_ReturnsUnifiedConflict_AndFreshRequestSucceeds(MembershipMutation mutation)
    {
        await factory.EnsureSystemUsersSeededAsync();
        var arrangementId = Guid.NewGuid();
        var existingBeerId = Guid.NewGuid();
        var addedBeerId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();
        var addedUserId = Guid.NewGuid();
        await SeedAsync(arrangementId, existingBeerId, addedBeerId, existingUserId, addedUserId);

        using var membershipClient = CreateAdminClient();
        using var statusClient = CreateAdminClient();
        await InstallMembershipDelayTriggerAsync(arrangementId);

        var membershipTask = SendMembershipAsync(
            membershipClient, mutation, arrangementId, existingBeerId, addedBeerId, existingUserId, addedUserId);
        await Task.Delay(TimeSpan.FromMilliseconds(500));

        var statusResponse = await statusClient.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/cancel", new { });
        Assert.Equal(HttpStatusCode.Created, statusResponse.StatusCode);

        var conflictResponse = await membershipTask;
        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
        using (var error = JsonDocument.Parse(await conflictResponse.Content.ReadAsStringAsync()))
        {
            Assert.Equal("conflict", error.RootElement.GetProperty("code").GetString());
            Assert.False(string.IsNullOrWhiteSpace(error.RootElement.GetProperty("message").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(error.RootElement.GetProperty("correlationId").GetString()));
        }

        using (var conflictScope = factory.Services.CreateScope())
        {
            var conflictDb = conflictScope.ServiceProvider.GetRequiredService<ArrangementDbContext>();
            var afterConflict = await conflictDb.Arrangements
                .Include(item => item.Beers)
                .Include(item => item.Participants)
                .SingleAsync(item => item.Id == arrangementId);
            Assert.Equal(ArrangementStatus.Canceled, afterConflict.Status);
            Assert.Single(afterConflict.Beers);
            Assert.Single(afterConflict.Participants);
        }

        var reopenResponse = await statusClient.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/reopen", new { });
        Assert.Equal(HttpStatusCode.Created, reopenResponse.StatusCode);
        var freshResponse = await SendMembershipAsync(
            membershipClient, mutation, arrangementId, existingBeerId, addedBeerId, existingUserId, addedUserId);
        var expectedFreshStatus = mutation is MembershipMutation.AddBeer or MembershipMutation.AddParticipant
            ? HttpStatusCode.Created
            : HttpStatusCode.OK;
        Assert.Equal(expectedFreshStatus, freshResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ArrangementDbContext>();
        var arrangement = await db.Arrangements
            .Include(item => item.Beers)
            .Include(item => item.Participants)
            .SingleAsync(item => item.Id == arrangementId);
        Assert.Equal(arrangement.Beers.Count, arrangement.Beers.Select(item => item.BeerId).Distinct().Count());
        Assert.Equal(arrangement.Participants.Count, arrangement.Participants.Select(item => item.UserId).Distinct().Count());
    }

    private HttpClient CreateAdminClient()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");
        return client;
    }

    private async Task InstallMembershipDelayTriggerAsync(Guid arrangementId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ArrangementDbContext>();
#pragma warning disable EF1002 // Test-only identifiers and UUIDs are generated locally.
        await db.Database.ExecuteSqlRawAsync($$"""
            CREATE OR REPLACE FUNCTION delay_membership_update_{{arrangementId:N}}()
            RETURNS trigger AS $trigger$
            BEGIN
                PERFORM pg_sleep(2);
                RETURN COALESCE(NEW, OLD);
            END;
            $trigger$ LANGUAGE plpgsql;
            CREATE TRIGGER delay_beer_insert_{{arrangementId:N}}
            BEFORE INSERT ON arrangement_beers
            FOR EACH ROW
            WHEN (NEW.arrangement_id = '{{arrangementId}}'::uuid)
            EXECUTE FUNCTION delay_membership_update_{{arrangementId:N}}();
            CREATE TRIGGER delay_beer_delete_{{arrangementId:N}}
            BEFORE DELETE ON arrangement_beers
            FOR EACH ROW
            WHEN (OLD.arrangement_id = '{{arrangementId}}'::uuid)
            EXECUTE FUNCTION delay_membership_update_{{arrangementId:N}}();
            CREATE TRIGGER delay_participant_insert_{{arrangementId:N}}
            BEFORE INSERT ON arrangement_participants
            FOR EACH ROW
            WHEN (NEW.arrangement_id = '{{arrangementId}}'::uuid)
            EXECUTE FUNCTION delay_membership_update_{{arrangementId:N}}();
            CREATE TRIGGER delay_participant_delete_{{arrangementId:N}}
            BEFORE DELETE ON arrangement_participants
            FOR EACH ROW
            WHEN (OLD.arrangement_id = '{{arrangementId}}'::uuid)
            EXECUTE FUNCTION delay_membership_update_{{arrangementId:N}}();
            """);
#pragma warning restore EF1002
    }

    private static Task<HttpResponseMessage> SendMembershipAsync(
        HttpClient client,
        MembershipMutation mutation,
        Guid arrangementId,
        Guid existingBeerId,
        Guid addedBeerId,
        Guid existingUserId,
        Guid addedUserId) => mutation switch
        {
            MembershipMutation.AddBeer => client.PostAsJsonAsync(
                $"/api/v1/arrangements/{arrangementId}/beers", new { beerId = addedBeerId }),
            MembershipMutation.RemoveBeer => client.DeleteAsync(
                $"/api/v1/arrangements/{arrangementId}/beers/{existingBeerId}"),
            MembershipMutation.AddParticipant => client.PostAsJsonAsync(
                $"/api/v1/arrangements/{arrangementId}/participants", new { userId = addedUserId }),
            MembershipMutation.RemoveParticipant => client.DeleteAsync(
                $"/api/v1/arrangements/{arrangementId}/participants/{existingUserId}"),
            _ => throw new ArgumentOutOfRangeException(nameof(mutation))
        };

    private async Task SeedAsync(
        Guid arrangementId,
        Guid existingBeerId,
        Guid addedBeerId,
        Guid existingUserId,
        Guid addedUserId)
    {
        await factory.SeedCatalogAsync(db =>
        {
            var style = new BeerStyle { Id = Guid.NewGuid(), Name = $"Style-{Guid.NewGuid()}", CreatedAt = DateTimeOffset.UtcNow };
            var type = new BeerType { Id = Guid.NewGuid(), Name = $"Type-{Guid.NewGuid()}", CreatedAt = DateTimeOffset.UtcNow };
            var brewery = new Brewery { Id = Guid.NewGuid(), Name = $"Brewery-{Guid.NewGuid()}", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
            db.AddRange(style, type, brewery);
            db.Beers.AddRange(
                NewCatalogBeer(existingBeerId, brewery.Id, style.Id, type.Id),
                NewCatalogBeer(addedBeerId, brewery.Id, style.Id, type.Id));
        });
        await factory.SeedUsersAsync(db => db.Users.AddRange(
            NewUser(existingUserId), NewUser(addedUserId)));
        await factory.SeedArrangementAsync(db =>
        {
            var arrangement = new ArrangementRecord
            {
                Id = arrangementId,
                Name = "Concurrent membership",
                Status = ArrangementStatus.Created,
                CreatedAt = DateTimeOffset.UtcNow
            };
            arrangement.Beers.Add(new ArrangementBeer
            {
                Id = Guid.NewGuid(), ArrangementId = arrangementId, BeerId = existingBeerId, CreatedAt = DateTimeOffset.UtcNow
            });
            arrangement.Participants.Add(new ArrangementParticipant
            {
                Id = Guid.NewGuid(), ArrangementId = arrangementId, UserId = existingUserId, CreatedAt = DateTimeOffset.UtcNow
            });
            db.Arrangements.Add(arrangement);
        });
    }

    private static Beer NewCatalogBeer(Guid id, Guid breweryId, Guid styleId, Guid typeId) => new()
    {
        Id = id, BreweryId = breweryId, BeerStyleId = styleId, BeerTypeId = typeId,
        Name = $"Beer-{id}", IsActive = true, CreatedAt = DateTimeOffset.UtcNow
    };

    private static User NewUser(Guid id) => new()
    {
        Id = id, Email = $"{id}@test.no", EmailNormalized = $"{id}@test.no",
        FirstName = "Test", LastName = "Participant", IsActive = true, Role = UserRole.User,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public enum MembershipMutation { AddBeer, RemoveBeer, AddParticipant, RemoveParticipant }
}
