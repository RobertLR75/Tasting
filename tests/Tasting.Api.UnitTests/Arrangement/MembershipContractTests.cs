using Tasting.Api.Features.Arrangement;
using Tasting.Api.Features.Arrangement.Beers.AddBeer;
using Tasting.Api.Features.Arrangement.Beers.RemoveBeer;
using Tasting.Api.Features.Arrangement.Participants.AddParticipant;
using Tasting.Api.Features.Arrangement.Participants.RemoveParticipant;
using Xunit;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class MembershipContractTests
{
    [Theory]
    [InlineData(typeof(AddBeerRequest))]
    [InlineData(typeof(RemoveBeerRequest))]
    [InlineData(typeof(AddParticipantRequest))]
    [InlineData(typeof(RemoveParticipantRequest))]
    [InlineData(typeof(ArrangementResponse))]
    public void MembershipApiContract_DoesNotExposeRowVersion(Type contractType)
    {
        Assert.DoesNotContain(
            contractType.GetProperties(),
            property => property.Name.Equals("RowVersion", StringComparison.OrdinalIgnoreCase));
    }
}
