using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Catalog.Beers.ListBeers;

public sealed record ListBeersQuery(bool IncludeInactive) : IRequest<ListBeersResult>;
