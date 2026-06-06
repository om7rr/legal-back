using LegalPlatform.SharedKernel.Classification;
using LegalPlatform.SharedKernel.Multitenancy;

namespace LegalPlatform.Modules.Cases.Domain;

/// <summary>
/// A legal case (the firm's system of record). Client identity lives in the Clients context and is
/// referenced by id here. Tenant-owned → global query filter + (later) Postgres RLS.
/// </summary>
[DataClassification(DataClassification.Confidential)]
public sealed class Case : ITenantOwned
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public string CaseNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Court { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public Guid LeadLawyerId { get; private set; }
    public string Status { get; private set; } = "open";
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private Case() { }

    public Case(
        Guid tenantId,
        string caseNumber,
        string title,
        string type,
        string court,
        Guid clientId,
        Guid leadLawyerId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant is required.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(caseNumber))
        {
            throw new ArgumentException("Case number is required.", nameof(caseNumber));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        TenantId = tenantId;
        CaseNumber = caseNumber.Trim();
        Title = title.Trim();
        Type = type.Trim();
        Court = court.Trim();
        ClientId = clientId;
        LeadLawyerId = leadLawyerId;
    }
}
