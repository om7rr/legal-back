namespace LegalPlatform.Modules.Cases.Contracts;

public sealed record CreateCaseRequest(
    string CaseNumber,
    string Title,
    string Type,
    string Court,
    Guid ClientId,
    Guid LeadLawyerId);

public sealed record CreateCaseResponse(Guid Id, string CaseNumber);

public sealed record CaseListItem(
    Guid Id,
    string CaseNumber,
    string Title,
    string Type,
    string Court,
    string Status);

public sealed record CaseDetails(
    Guid Id,
    string CaseNumber,
    string Title,
    string Type,
    string Court,
    Guid ClientId,
    Guid LeadLawyerId,
    string Status,
    DateTimeOffset CreatedAt);
