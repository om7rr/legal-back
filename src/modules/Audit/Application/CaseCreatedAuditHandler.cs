using System.Security.Cryptography;
using System.Text;
using LegalPlatform.Modules.Audit.Domain;
using LegalPlatform.SharedKernel.Events;
using LegalPlatform.SharedKernel.IntegrationEvents;
using LegalPlatform.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LegalPlatform.Modules.Audit.Application;

/// <summary>
/// Appends a tamper-evident, hash-chained audit entry when a case is created. Consumes the shared
/// integration event — no dependency on the Cases module.
/// </summary>
public sealed class CaseCreatedAuditHandler : IIntegrationEventHandler<CaseCreatedIntegrationEvent>
{
    private const string Action = "case.created";
    private const string EntityType = "Case";

    private readonly IAppDbContext _db;

    public CaseCreatedAuditHandler(IAppDbContext db) => _db = db;

    public async Task HandleAsync(CaseCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var previousHash = await _db.Set<AuditEntry>()
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == integrationEvent.TenantId)
            .OrderByDescending(a => a.OccurredAt)
            .Select(a => a.Hash)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var entityId = integrationEvent.CaseId.ToString();
        var payload = $"{previousHash}|{Action}|{EntityType}|{entityId}|{integrationEvent.OccurredAt:O}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));

        _db.Set<AuditEntry>().Add(new AuditEntry(
            integrationEvent.TenantId,
            Action,
            EntityType,
            entityId,
            integrationEvent.ActorId,
            previousHash,
            hash));

        await _db.SaveChangesAsync(cancellationToken);
    }
}
