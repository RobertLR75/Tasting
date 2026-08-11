using System.Reflection;
using Tasting.Admin.Features.Identity.Models;
using Tasting.Admin.Features.Catalog.Models;
using Tasting.Admin.Features.Arrangement.Models;
using Tasting.Admin.UnitTests.Builders;

namespace Tasting.Admin.UnitTests;

public sealed class ApiContractTests
{
    [Fact]
    public void FrontendFeatureModels_ShouldNotExposeInternalVersions()
    {
        var adminAssembly = Assembly.Load("Tasting.Admin");
        var forbiddenProperties = adminAssembly.GetTypes()
            .Where(type => type.IsPublic && type.Namespace?.StartsWith("Tasting.Admin.Features", StringComparison.Ordinal) == true)
            .SelectMany(type => type.GetProperties()
                .Where(property => property.Name.Contains("Version", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Contains("ETag", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Contains("ConcurrencyToken", StringComparison.OrdinalIgnoreCase))
                .Select(property => $"{type.FullName}.{property.Name}"))
            .ToArray();

        Assert.Empty(forbiddenProperties);
    }

    [Fact]
    public void UserDto_ShouldHaveAllRequiredProperties()
    {
        var user = UserDtoBuilder.Admin().Build();
        
        Assert.NotNull(user.FirstName);
        Assert.NotNull(user.LastName);
        Assert.NotNull(user.Email);
        Assert.NotNull(user.Role);
        Assert.NotNull(user.Status);
    }

    [Fact]
    public void BreweryDto_ShouldHaveAllRequiredProperties()
    {
        var brewery = BreweryDtoBuilder.Default().Build();
        
        Assert.NotEqual(Guid.Empty, brewery.Id);
        Assert.NotNull(brewery.Name);
        Assert.False(brewery.Name == "");
        Assert.NotEqual(default(DateTimeOffset), brewery.CreatedAt);
    }

    [Fact]
    public void BeerDto_ShouldHaveAllRequiredProperties()
    {
        var beer = BeerDtoBuilder.Default().Build();
        
        Assert.NotEqual(Guid.Empty, beer.Id);
        Assert.NotEqual(Guid.Empty, beer.BreweryId);
        Assert.NotNull(beer.Name);
        Assert.NotEqual(default(DateTimeOffset), beer.CreatedAt);
    }

    [Fact]
    public void ArrangementDto_ShouldHaveAllRequiredProperties()
    {
        var arrangement = ArrangementDtoBuilder.Default().Build();
        
        Assert.NotEqual(Guid.Empty, arrangement.Id);
        Assert.NotNull(arrangement.Name);
        Assert.Equal(ArrangementStatus.Created, arrangement.Status);
        Assert.NotEqual(default(DateTimeOffset), arrangement.CreatedAt);
    }

    [Fact]
    public void UserDtoBuilder_ShouldSupportMultipleFluentChains()
    {
        var user = UserDtoBuilder.Default()
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .WithEmail("jane@example.com")
            .WithRole("Admin")
            .WithStatus("Active")
            .Build();
        
        Assert.Equal("Jane", user.FirstName);
        Assert.Equal("Smith", user.LastName);
        Assert.Equal("jane@example.com", user.Email);
        Assert.Equal("Admin", user.Role);
        Assert.Equal("Active", user.Status);
    }

    [Fact]
    public void BreweryDtoBuilder_ShouldSupportQuickStatusConfigurations()
    {
        var active = BreweryDtoBuilder.Default().Build();
        var inactive = BreweryDtoBuilder.Inactive().Build();
        
        Assert.True(active.IsActive);
        Assert.False(inactive.IsActive);
    }

    [Fact]
    public void ArrangementDtoBuilder_ShouldSupportAllStatusConfigurations()
    {
        var created = ArrangementDtoBuilder.Default().Build();
        var active = ArrangementDtoBuilder.Active().Build();
        var started = ArrangementDtoBuilder.Started().Build();
        var completed = ArrangementDtoBuilder.Completed().Build();
        var canceled = ArrangementDtoBuilder.Canceled().Build();
        
        Assert.Equal(ArrangementStatus.Created, created.Status);
        Assert.Equal(ArrangementStatus.Active, active.Status);
        Assert.Equal(ArrangementStatus.Started, started.Status);
        Assert.Equal(ArrangementStatus.Completed, completed.Status);
        Assert.Equal(ArrangementStatus.Canceled, canceled.Status);
    }

    [Fact]
    public void ArrangementStatus_ShouldMatchBackendEnumOrdering()
    {
        Assert.Equal(0, (int)ArrangementStatus.Created);
        Assert.Equal(1, (int)ArrangementStatus.Active);
        Assert.Equal(2, (int)ArrangementStatus.Started);
        Assert.Equal(3, (int)ArrangementStatus.Canceled);
        Assert.Equal(4, (int)ArrangementStatus.Completed);
    }

    [Fact]
    public void CreateUserRequest_ShouldRequireValidData()
    {
        var request = new AddUserRequest("John", "Doe", "john@example.com", "password");
        
        Assert.NotNull(request.FirstName);
        Assert.NotNull(request.LastName);
        Assert.NotNull(request.Email);
        Assert.NotNull(request.Password);
    }

    [Fact]
    public void CreateBreweryRequest_ShouldRequireValidName()
    {
        var request = new CreateBreweryRequest("Brewery Name");
        
        Assert.NotNull(request.Name);
        Assert.False(request.Name == "");
    }

    [Fact]
    public void CreateArrangementRequest_ShouldAllowOptionalDescription()
    {
        var withDescription = new CreateArrangementRequest("Event", "Description here");
        var withoutDescription = new CreateArrangementRequest("Event", null);
        
        Assert.NotNull(withDescription.Name);
        Assert.NotNull(withDescription.Description);
        Assert.NotNull(withoutDescription.Name);
        Assert.Null(withoutDescription.Description);
    }

    [Fact]
    public void ArrangementLifecycleRequest_ShouldNotExposeRowVersion()
    {
        var request = new ArrangementLifecycleRequest();
        Assert.DoesNotContain(request.GetType().GetProperties(), property => property.Name == "RowVersion");
    }

    [Fact]
    public void ArrangementContracts_ShouldNotExposeRowVersion()
    {
        Assert.DoesNotContain(typeof(AddBeerToArrangementRequest).GetProperties(), property => property.Name == "RowVersion");
        Assert.DoesNotContain(typeof(AddParticipantToArrangementRequest).GetProperties(), property => property.Name == "RowVersion");
        Assert.DoesNotContain(typeof(ArrangementDto).GetProperties(), property => property.Name == "RowVersion");
        Assert.DoesNotContain(typeof(UpdateArrangementRequest).GetProperties(), property => property.Name == "RowVersion");
    }
}
