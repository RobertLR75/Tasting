using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Contracts;
using Tasting.Api.Features.Rating.Ratings.SubmitRating;
using Tasting.Api.Features.Rating.Results.GetResults;
using RatingEntity = Tasting.Api.Infrastructure.Rating.Entities.Rating;

namespace Tasting.Api.Infrastructure.Rating;

public static class RatingServiceExtensions
{
    public static void AddRatingServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var connectionString = builder.Configuration.GetConnectionString("TastingDb");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            builder.Services.AddDbContext<RatingDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        builder.Services.AddScoped<IArrangementService, StubArrangementService>();
        builder.Services.AddScoped<IRequestHandler<SubmitRatingCommand, RatingEntity>, SubmitRatingHandler>();
        builder.Services.AddScoped<IRequestHandler<GetResultsQuery, GetResultsResponse>, GetResultsHandler>();
    }
}
