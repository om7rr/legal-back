using LegalPlatform.Modules.Cases.Contracts;
using LegalPlatform.Modules.Cases.Domain;
using LegalPlatform.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LegalPlatform.Modules.Cases.Application;

/// <summary>
/// Fetches one case by id. The tenant query filter means another tenant's id simply yields null
/// (→ 404), never a cross-tenant read.
/// </summary>
public sealed class GetCaseByIdHandler
{
    private readonly IAppDbContext _db;

    public GetCaseByIdHandler(IAppDbContext db) => _db = db;

    public async Task<CaseDetails?> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Set<Case>()
            .Where(c => c.Id == id)
            .Select(c => new CaseDetails(
                c.Id, c.CaseNumber, c.Title, c.Type, c.Court, c.ClientId, c.LeadLawyerId, c.Status, c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
