using Microsoft.EntityFrameworkCore;

namespace LegalPlatform.SharedKernel.Persistence;

/// <summary>
/// Narrow persistence abstraction so bounded contexts get typed data access without depending on
/// Infrastructure (ADR-0005). Implemented by the shared AppDbContext.
/// </summary>
public interface IAppDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
