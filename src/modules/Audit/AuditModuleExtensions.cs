using LegalPlatform.Modules.Audit.Application;
using LegalPlatform.SharedKernel.Events;
using LegalPlatform.SharedKernel.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;

namespace LegalPlatform.Modules.Audit;

/// <summary>DI registration for the Audit context (integration-event handlers).</summary>
public static class AuditModuleExtensions
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventHandler<CaseCreatedIntegrationEvent>, CaseCreatedAuditHandler>();
        return services;
    }
}
