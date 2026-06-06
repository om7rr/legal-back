using LegalPlatform.Modules.Cases.Application;
using Microsoft.Extensions.DependencyInjection;

namespace LegalPlatform.Modules.Cases;

/// <summary>DI registration for the Cases context.</summary>
public static class CasesModuleExtensions
{
    public static IServiceCollection AddCasesModule(this IServiceCollection services)
    {
        services.AddScoped<CreateCaseHandler>();
        services.AddScoped<GetCasesHandler>();
        services.AddScoped<GetCaseByIdHandler>();
        return services;
    }
}
