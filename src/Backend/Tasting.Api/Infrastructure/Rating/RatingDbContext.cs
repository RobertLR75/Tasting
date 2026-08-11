using Microsoft.EntityFrameworkCore;
using Tasting.Api.Infrastructure.Rating.Entities;

namespace Tasting.Api.Infrastructure.Rating;

public class RatingDbContext(DbContextOptions<RatingDbContext> options) : DbContext(options)
{
    public DbSet<RatingRecord> Ratings { get; set; } = null!;
    public DbSet<Result> Results { get; set; } = null!;
    public DbSet<ResultParticipant> ResultParticipants { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureRating(modelBuilder);
        ConfigureResult(modelBuilder);
        ConfigureResultParticipant(modelBuilder);
    }

    private static void ConfigureRating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RatingRecord>(b =>
        {
            b.ToTable("ratings");
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
            b.Property(r => r.ArrangementId).HasColumnName("arrangement_id").IsRequired();
            b.Property(r => r.ParticipantId).HasColumnName("participant_id").IsRequired();
            b.Property(r => r.BeerId).HasColumnName("beer_id").IsRequired();
            b.Property(r => r.Visibility).HasColumnName("visibility").HasPrecision(4, 1).IsRequired();
            b.Property(r => r.Smell).HasColumnName("smell").HasPrecision(4, 1).IsRequired();
            b.Property(r => r.Taste).HasColumnName("taste").HasPrecision(4, 1).IsRequired();
            b.Property(r => r.Toast).HasColumnName("toast").HasPrecision(4, 1).IsRequired();
            b.Property(r => r.TotalRating).HasColumnName("total_rating").HasPrecision(5, 2).IsRequired();
            b.Property(r => r.RowVersion).HasColumnName("row_version").IsConcurrencyToken().IsRequired();
            b.Property(r => r.CreatedAt).HasColumnName("created_at_utc").IsRequired();
            b.Property(r => r.UpdatedAt).HasColumnName("updated_at_utc");

            b.HasIndex(r => new { r.ArrangementId, r.ParticipantId, r.BeerId })
                .IsUnique()
                .HasDatabaseName("ix_ratings_arrangement_participant_beer");
        });
    }

    private static void ConfigureResult(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Result>(b =>
        {
            b.ToTable("results");
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
            b.Property(r => r.ArrangementId).HasColumnName("arrangement_id").IsRequired();
            b.Property(r => r.BeerId).HasColumnName("beer_id").IsRequired();
            b.Property(r => r.BeerNameSnapshot).HasColumnName("beer_name_snapshot").HasMaxLength(200).IsRequired();
            b.Property(r => r.TotalRating).HasColumnName("total_rating").HasPrecision(5, 2).IsRequired();
            b.Property(r => r.RatingCount).HasColumnName("rating_count").IsRequired();
            b.Property(r => r.StandardDeviation).HasColumnName("standard_deviation").HasPrecision(10, 6).IsRequired();
            b.Property(r => r.Rank).HasColumnName("rank").IsRequired();
            b.Property(r => r.CreatedAt).HasColumnName("created_at_utc").IsRequired();
            b.Property(r => r.UpdatedAt).HasColumnName("updated_at_utc");

            b.HasMany(r => r.Participants)
                .WithOne()
                .HasForeignKey(rp => rp.ResultId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(r => new { r.ArrangementId, r.BeerId })
                .IsUnique()
                .HasDatabaseName("ix_results_arrangement_beer");
        });
    }

    private static void ConfigureResultParticipant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ResultParticipant>(b =>
        {
            b.ToTable("result_participants");
            b.HasKey(rp => rp.Id);
            b.Property(rp => rp.Id).HasColumnName("id").ValueGeneratedNever();
            b.Property(rp => rp.ResultId).HasColumnName("result_id").IsRequired();
            b.Property(rp => rp.ParticipantId).HasColumnName("participant_id").IsRequired();
            b.Property(rp => rp.ParticipantNameSnapshot).HasColumnName("participant_name_snapshot").HasMaxLength(200).IsRequired();
            b.Property(rp => rp.Rating).HasColumnName("rating").HasPrecision(5, 2).IsRequired();

            b.HasIndex(rp => new { rp.ResultId, rp.ParticipantId })
                .IsUnique()
                .HasDatabaseName("ix_result_participants_result_participant");
        });
    }
}
