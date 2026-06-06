using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LegalPlatform.Infrastructure.Persistence;

/// <summary>
/// Model cache key that includes the set of module assemblies (<see cref="AppDbContext.ModuleSetKey"/>).
/// Without this, EF caches one model per context type, so a context built with a different module set
/// (e.g. in tests) would wrongly reuse another's cached model.
/// </summary>
public sealed class ModuleAwareModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
        => context is AppDbContext app
            ? (context.GetType(), app.ModuleSetKey, designTime)
            : (context.GetType(), designTime);
}
