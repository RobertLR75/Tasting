using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Contracts;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Features.Rating.Ratings.SubmitRating;
using Tasting.Api.Features.Rating.Results.GetResults;
using DomainRating = Tasting.Api.Features.Rating.Domain.Rating;

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
        else
        {
            builder.Services.AddDbContext<RatingDbContext>(options =>
                options.UseInMemoryDatabase("tasting-rating"));
        }

        builder.Services.AddScoped<IRequestHandler<SubmitRatingCommand, DomainRating>, SubmitRatingHandler>();
        builder.Services.AddScoped<IRequestHandler<GetResultsQuery, GetResultsResponse>, GetResultsHandler>();
    }
}
