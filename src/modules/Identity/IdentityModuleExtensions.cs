using LegalPlatform.Modules.Identity.Application;
using LegalPlatform.Modules.Identity.Auth;
using LegalPlatform.Modules.Identity.Nafath;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LegalPlatform.Modules.Identity;

/// <summary>DI registration for the Identity &amp; Access context (ADR-0006).</summary>
public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton(JwtSetup.Resolve(configuration, environment));
        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();
        services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();

        // Mock Nafath (test environment). The real adapter replaces these registrations later.
        services.AddSingleton<MockNafathClient>();
        services.AddSingleton<INafathClient>(sp => sp.GetRequiredService<MockNafathClient>());
        services.AddSingleton<INafathSimulator>(sp => sp.GetRequiredService<MockNafathClient>());

        services.AddScoped<NafathAuthService>();
        return services;
    }
}
