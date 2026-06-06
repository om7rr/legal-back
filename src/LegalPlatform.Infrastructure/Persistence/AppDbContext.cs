using System.Reflection;
using LegalPlatform.SharedKernel.Multitenancy;
using LegalPlatform.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LegalPlatform.Infrastructure.Persistence;

/// <summary>
/// Shared persistence context for the modular monolith. Each bounded context contributes its own
/// <c>IEntityTypeConfiguration</c>s (discovered via <see cref="ModuleRegistry"/>); this context adds
/// the tenant query filter applied to every <see cref="ITenantOwned"/> entity. Defense-in-depth
/// alongside Postgres Row-Level Security (ADR-0002).
/// </summary>
public sealed class AppDbContext : DbContext, IAppDbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly ModuleRegistry _moduleRegistry;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantContext tenantContext,
        ModuleRegistry moduleRegistry)
        : base(options)
    {
        _tenantContext = tenantContext;
        _moduleRegistry = moduleRegistry;
    }

    /// <summary>
    /// Identifies the set of module assemblies this context's model is built from. Part of the model
    /// cache key so contexts configured with different module sets don't share a cached model.
    /// </summary>
    internal string ModuleSetKey =>
        string.Join(",", _moduleRegistry.ModuleAssemblies
            .Select(a => a.FullName)
            .OrderBy(n => n, StringComparer.Ordinal));

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ReplaceService<IModelCacheKeyFactory, ModuleAwareModelCacheKeyFactory>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var assembly in _moduleRegistry.ModuleAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }

        ApplyTenantQueryFilters(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantOwned).IsAssignableFrom(entityType.ClrType))
            {
                typeof(AppDbContext)
                    .GetMethod(nameof(SetTenantFilter), BindingFlags.Instance | BindingFlags.NonPublic)!
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, [modelBuilder]);
            }
        }
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantOwned
    {
        // Captures the context instance; EF re-reads the current tenant id per query.
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
    }
}
