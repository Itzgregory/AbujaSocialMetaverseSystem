using System.Linq.Expressions;
using AbujaSocialMetaverse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Infrastructure.Data;

/// <summary>
/// Application DbContext - lives in Infrastructure layer only.
/// No domain logic. Only DbSet definitions and entity configuration.
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly ILogger<ApplicationDbContext> _logger;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ILogger<ApplicationDbContext> logger) : base(options)
    {
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _logger.LogDebug("Applying entity configurations from modules...");

        // Scan ONLY modules assembly pattern
        var moduleAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName!.Contains("AbujaSocialMetaverse.Modules"))
            .ToList();

        foreach (var assembly in moduleAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            _logger.LogDebug("Applied configurations from {Assembly}", assembly.GetName().Name);
        }

        // Apply soft delete global query filter for ISoftDeletable entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var condition = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);

                entityType.SetQueryFilter(lambda);
                _logger.LogDebug("Applied soft delete filter to {Entity}", entityType.ClrType.Name);
            }
        }

        _logger.LogDebug("Entity configuration complete.");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Use timestamptz for all DateTimeOffset columns
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveColumnType("timestamptz");
    }
}