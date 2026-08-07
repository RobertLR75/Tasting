using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Contracts;
using Tasting.Api.Features.Arrangement.Arrangements.ActivateArrangement;
using Tasting.Api.Features.Arrangement.Arrangements.CancelArrangement;
using Tasting.Api.Features.Arrangement.Arrangements.CompleteArrangement;
using Tasting.Api.Features.Arrangement.Arrangements.CreateArrangement;
using Tasting.Api.Features.Arrangement.Arrangements.GetArrangement;
using Tasting.Api.Features.Arrangement.Arrangements.ListArrangements;
using Tasting.Api.Features.Arrangement.Arrangements.StartArrangement;
using Tasting.Api.Features.Arrangement.Arrangements.UpdateArrangement;
using Tasting.Api.Features.Arrangement.Beers.AddBeer;
using Tasting.Api.Features.Arrangement.Beers.RemoveBeer;
using Tasting.Api.Features.Arrangement.Participants.AddParticipant;
using Tasting.Api.Features.Arrangement.Participants.RemoveParticipant;
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
            IRequestHandler<ActivateArrangementCommand, ArrangementEntity>,
            ActivateArrangementHandler>();

        services.AddScoped<
            IRequestHandler<StartArrangementCommand, ArrangementEntity>,
            StartArrangementHandler>();

        services.AddScoped<
            IRequestHandler<GetArrangementQuery, ArrangementEntity>,
            GetArrangementHandler>();

        services.AddScoped<
            IRequestHandler<ListArrangementsQuery, ListArrangementsResult>,
            ListArrangementsHandler>();

        services.AddScoped<
            IRequestHandler<UpdateArrangementCommand, ArrangementEntity>,
            UpdateArrangementHandler>();

        services.AddScoped<
            IRequestHandler<CancelArrangementCommand, ArrangementEntity>,
            CancelArrangementHandler>();

        services.AddScoped<
            IRequestHandler<CompleteArrangementCommand, ArrangementEntity>,
            CompleteArrangementHandler>();

        services.AddScoped<
            IRequestHandler<RemoveParticipantCommand, ArrangementEntity>,
            RemoveParticipantHandler>();

        services.AddScoped<
            IRequestHandler<RemoveBeerCommand, ArrangementEntity>,
            RemoveBeerHandler>();

        return services;
    }
}
