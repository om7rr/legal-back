using LegalPlatform.Modules.Cases.Contracts;
using LegalPlatform.Modules.Cases.Domain;
using LegalPlatform.SharedKernel.Events;
using LegalPlatform.SharedKernel.IntegrationEvents;
using LegalPlatform.SharedKernel.Multitenancy;
using LegalPlatform.SharedKernel.Persistence;
using LegalPlatform.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace LegalPlatform.Modules.Cases.Application;

/// <summary>Creates a case for the current tenant and publishes <see cref="CaseCreatedIntegrationEvent"/>.</summary>
public sealed class CreateCaseHandler
{
    private readonly IAppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IEventBus _eventBus;

    public CreateCaseHandler(IAppDbContext db, ITenantContext tenant, IEventBus eventBus)
    {
        _db = db;
        _tenant = tenant;
        _eventBus = eventBus;
    }

    public async Task<Result<CreateCaseResponse>> HandleAsync(
        CreateCaseRequest request,
        Guid? actorId,
        CancellationToken cancellationToken = default)
    {
        if (!_tenant.HasTenant)
        {
            return Result.Failure<CreateCaseResponse>(Error.Forbidden("A tenant context is required."));
        }

        if (string.IsNullOrWhiteSpace(request.CaseNumber))
        {
            return Result.Failure<CreateCaseResponse>(Error.Validation("Case number is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Result.Failure<CreateCaseResponse>(Error.Validation("Title is required."));
        }

        // Query filter scopes this to the current tenant.
        var duplicate = await _db.Set<Case>().AnyAsync(c => c.CaseNumber == request.CaseNumber, cancellationToken);
        if (duplicate)
        {
            return Result.Failure<CreateCaseResponse>(Error.Conflict($"Case number '{request.CaseNumber}' already exists."));
        }

        var entity = new Case(
            _tenant.TenantId,
            request.CaseNumber,
            request.Title,
            request.Type,
            request.Court,
            request.ClientId,
            request.LeadLawyerId);

        _db.Set<Case>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new CaseCreatedIntegrationEvent(entity.Id, entity.TenantId, entity.CaseNumber, actorId),
            cancellationToken);

        return Result.Success(new CreateCaseResponse(entity.Id, entity.CaseNumber));
    }
}
