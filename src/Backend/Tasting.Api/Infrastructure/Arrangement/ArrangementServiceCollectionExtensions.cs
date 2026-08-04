using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Contracts;
using Tasting.Api.Features.Arrangement.Arrangements.CreateArrangement;
using Tasting.Api.Features.Arrangement.Arrangements.StartArrangement;
using Tasting.Api.Features.Arrangement.Beers.AddBeer;
using Tasting.Api.Features.Arrangement.Participants.AddParticipant;
using ArrangementEntity = Tasting.Api.Features.Arrangement.Domain.Arrangement;

namespace Tasting.Api.Infrastructure.Arrangement;

public static class ArrangementServiceCollectionExtensions
{
    public static IServiceCollection AddArrangement(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TastingDb");
        services.AddDbContext<ArrangementDbContext>(options =>
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseInMemoryDatabase("tasting-arrangement");
                return;
            }

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IArrangementService, ArrangementService>();

        services.AddScoped<
            IRequestHandler<CreateArrangementCommand, ArrangementEntity>,
            CreateArrangementHandler>();

        services.AddScoped<
            IRequestHandler<AddParticipantCommand, ArrangementEntity>,
            AddParticipantHandler>();

        services.AddScoped<
            IRequestHandler<AddBeerCommand, ArrangementEntity>,
            AddBeerHandler>();

        services.AddScoped<
            IRequestHandler<StartArrangementCommand, ArrangementEntity>,
            StartArrangementHandler>();

        return services;
    }
}
