using Microsoft.EntityFrameworkCore;
using Tasting.Api.Features.Arrangement.Domain;

namespace Tasting.Api.Infrastructure.Arrangement;

public sealed class ArrangementDbContext(DbContextOptions<ArrangementDbContext> options) : DbContext(options)
{
    public DbSet<ArrangementRecord> Arrangements => Set<ArrangementRecord>();
    public DbSet<ArrangementParticipant> Participants => Set<ArrangementParticipant>();
    public DbSet<ArrangementBeer> Beers => Set<ArrangementBeer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureArrangement(modelBuilder.Entity<ArrangementRecord>());
        ConfigureParticipant(modelBuilder.Entity<ArrangementParticipant>());
        ConfigureBeer(modelBuilder.Entity<ArrangementBeer>());

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureArrangement(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ArrangementRecord> builder)
    {
        builder.ToTable("arrangements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(x => x.RowVersion).HasColumnName("row_version").IsRequired().IsConcurrencyToken();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at_utc");

        builder.HasMany(x => x.Participants)
            .WithOne()
            .HasForeignKey(p => p.ArrangementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Beers)
            .WithOne()
            .HasForeignKey(b => b.ArrangementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Status).HasDatabaseName("ix_arrangements_status");
    }

    private static void ConfigureParticipant(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ArrangementParticipant> builder)
    {
        builder.ToTable("arrangement_participants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.ArrangementId).HasColumnName("arrangement_id").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.FirstNameSnapshot).HasColumnName("first_name_snapshot").HasMaxLength(200).IsRequired();
        builder.Property(x => x.LastNameSnapshot).HasColumnName("last_name_snapshot").HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(x => new { x.ArrangementId, x.UserId })
            .IsUnique()
            .HasDatabaseName("uix_arrangement_participants_arrangement_user");
    }

    private static void ConfigureBeer(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ArrangementBeer> builder)
    {
        builder.ToTable("arrangement_beers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.ArrangementId).HasColumnName("arrangement_id").IsRequired();
        builder.Property(x => x.BeerId).HasColumnName("beer_id").IsRequired();
        builder.Property(x => x.NameSnapshot).HasColumnName("name_snapshot").HasMaxLength(200).IsRequired();
        builder.Property(x => x.BreweryNameSnapshot).HasColumnName("brewery_name_snapshot").HasMaxLength(200).IsRequired();
        builder.Property(x => x.BeerStyleSnapshot).HasColumnName("beer_style_snapshot").HasMaxLength(200).IsRequired();
        builder.Property(x => x.BeerTypeSnapshot).HasColumnName("beer_type_snapshot").HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(x => new { x.ArrangementId, x.BeerId })
            .IsUnique()
            .HasDatabaseName("uix_arrangement_beers_arrangement_beer");
    }
}
