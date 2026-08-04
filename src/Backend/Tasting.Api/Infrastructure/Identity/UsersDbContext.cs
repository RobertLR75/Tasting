using Microsoft.EntityFrameworkCore;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Infrastructure.Identity;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var userBuilder = modelBuilder.Entity<User>();
        userBuilder.ToTable("users", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("ck_users_role", "\"role\" IN ('Admin', 'User')");
        });

        userBuilder.HasKey(user => user.Id);
        userBuilder.Property(user => user.Id).HasColumnName("id").ValueGeneratedNever();
        userBuilder.Property(user => user.Email).HasColumnName("email").HasMaxLength(200).IsRequired();
        userBuilder.Property(user => user.EmailNormalized).HasColumnName("email_normalized").HasMaxLength(200).IsRequired();
        userBuilder.Property(user => user.FirstName).HasColumnName("first_name").HasMaxLength(200).IsRequired();
        userBuilder.Property(user => user.LastName).HasColumnName("last_name").HasMaxLength(200).IsRequired();
        userBuilder.Property(user => user.Role).HasColumnName("role").HasMaxLength(20).HasConversion<string>().IsRequired();
        userBuilder.Property(user => user.IsActive).HasColumnName("is_active").IsRequired();
        userBuilder.Property(user => user.CreatedAt).HasColumnName("created_at_utc").IsRequired();
        userBuilder.Property(user => user.UpdatedAt).HasColumnName("updated_at_utc");

        userBuilder.HasIndex(user => user.EmailNormalized)
            .IsUnique()
            .HasDatabaseName("ix_users_email_normalized");
    }
}
