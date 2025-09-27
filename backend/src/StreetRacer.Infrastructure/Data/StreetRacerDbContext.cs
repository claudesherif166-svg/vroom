using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using StreetRacer.Domain.Common;
using StreetRacer.Domain.Entities;
using System.Linq.Expressions;

namespace StreetRacer.Infrastructure.Data;

public class StreetRacerDbContext : DbContext
{
    public StreetRacerDbContext(DbContextOptions<StreetRacerDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<PostComment> PostComments => Set<PostComment>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventContributor> EventContributors => Set<EventContributor>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<RaceRacer> RaceRacers => Set<RaceRacer>();
    public DbSet<RacerLocationUpdate> RacerLocationUpdates => Set<RacerLocationUpdate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure spatial data
        modelBuilder.HasPostgresExtension("postgis");

        // User configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.KeycloakSubject).IsUnique();
            
            entity.HasMany(e => e.Followers)
                  .WithOne(e => e.Followee)
                  .HasForeignKey(e => e.FolloweeId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Following)
                  .WithOne(e => e.Follower)
                  .HasForeignKey(e => e.FollowerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Follow configurations
        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasIndex(e => new { e.FollowerId, e.FolloweeId }).IsUnique();
        });

        // Race configurations
        modelBuilder.Entity<Race>(entity =>
        {
            entity.HasIndex(e => e.StartPoint).HasMethod("GIST");
            entity.HasIndex(e => e.Route).HasMethod("GIST");
            
            entity.Property(e => e.StartPoint)
                  .HasColumnType("geography (point)")
                  .IsRequired();
                  
            entity.Property(e => e.EndPoint)
                  .HasColumnType("geography (point)")
                  .IsRequired();
                  
            entity.Property(e => e.Route)
                  .HasColumnType("geography (linestring)");
        });

        // RaceRacer configurations
        modelBuilder.Entity<RaceRacer>(entity =>
        {
            entity.HasIndex(e => new { e.RaceId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.RaceId);
        });

        // RacerLocationUpdate configurations
        modelBuilder.Entity<RacerLocationUpdate>(entity =>
        {
            entity.HasIndex(e => new { e.RaceId, e.RecordedAt });
            entity.HasIndex(e => new { e.RacerUserId, e.RecordedAt });
        });

        // Event configurations
        modelBuilder.Entity<Event>(entity =>
        {
            entity.Property(e => e.Location)
                  .HasColumnType("geography (point)");
                  
            entity.HasIndex(e => e.Location).HasMethod("GIST");
        });

        // EventContributor configurations
        modelBuilder.Entity<EventContributor>(entity =>
        {
            entity.HasIndex(e => new { e.EventId, e.UserId }).IsUnique();
        });

        // Post configurations
        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique();
        });

        // Story configurations
        modelBuilder.Entity<Story>(entity =>
        {
            entity.HasIndex(e => e.ExpiresAt);
        });

        // Achievement configurations
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasIndex(e => e.Key).IsUnique();
        });

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.AchievementId }).IsUnique();
        });

        // Apply soft delete filter globally
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IDataModel).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(IDataModel.DeletedAt));
                var condition = Expression.Equal(property, Expression.Constant(null));
                var lambda = Expression.Lambda(condition, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IDataModel && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (IDataModel)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}