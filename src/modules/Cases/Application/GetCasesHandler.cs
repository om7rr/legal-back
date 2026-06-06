using LegalPlatform.Modules.Cases.Contracts;
using LegalPlatform.Modules.Cases.Domain;
using LegalPlatform.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LegalPlatform.Modules.Cases.Application;

/// <summary>Lists the current tenant's cases (tenant scoping enforced by the global query filter).</summary>
public sealed class GetCasesHandler
{
    private readonly IAppDbContext _db;

    public GetCasesHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CaseListItem>> HandleAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Set<Case>()
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CaseListItem(c.Id, c.CaseNumber, c.Title, c.Type, c.Court, c.Status))
            .ToListAsync(cancellationToken);
    }
}
