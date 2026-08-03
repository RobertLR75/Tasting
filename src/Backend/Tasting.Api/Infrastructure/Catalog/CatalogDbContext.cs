using Microsoft.EntityFrameworkCore;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Infrastructure.Catalog;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Brewery> Breweries => Set<Brewery>();
    public DbSet<Beer> Beers => Set<Beer>();
    public DbSet<BeerStyle> BeerStyles => Set<BeerStyle>();
    public DbSet<BeerType> BeerTypes => Set<BeerType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureBrewery(modelBuilder.Entity<Brewery>());
        ConfigureBeerStyle(modelBuilder.Entity<BeerStyle>());
        ConfigureBeerType(modelBuilder.Entity<BeerType>());
        ConfigureBeer(modelBuilder.Entity<Beer>());

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureBrewery(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Brewery> builder)
    {
        builder.ToTable("breweries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at_utc");
    }

    private static void ConfigureBeerStyle(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<BeerStyle> builder)
    {
        builder.ToTable("beer_styles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at_utc");
    }

    private static void ConfigureBeerType(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<BeerType> builder)
    {
        builder.ToTable("beer_types");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at_utc");
    }

    private static void ConfigureBeer(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Beer> builder)
    {
        builder.ToTable("beers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.BreweryId).HasColumnName("brewery_id").IsRequired();
        builder.Property(x => x.BeerStyleId).HasColumnName("beer_style_id").IsRequired();
        builder.Property(x => x.BeerTypeId).HasColumnName("beer_type_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at_utc");

        builder.HasOne(x => x.Brewery)
            .WithMany(x => x.Beers)
            .HasForeignKey(x => x.BreweryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BeerStyle)
            .WithMany(x => x.Beers)
            .HasForeignKey(x => x.BeerStyleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BeerType)
            .WithMany(x => x.Beers)
            .HasForeignKey(x => x.BeerTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BreweryId).HasDatabaseName("ix_beers_brewery_id");
        builder.HasIndex(x => x.BeerStyleId).HasDatabaseName("ix_beers_beer_style_id");
        builder.HasIndex(x => x.BeerTypeId).HasDatabaseName("ix_beers_beer_type_id");
    }
}
