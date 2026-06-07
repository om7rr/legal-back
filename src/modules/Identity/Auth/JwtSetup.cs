using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace LegalPlatform.Modules.Identity.Auth;

/// <summary>
/// Resolves <see cref="JwtOptions"/> from configuration. The signing key MUST be configured in
/// Production (startup fails otherwise); outside Production a dev-only key is used for the test
/// environment (ADR-0006). Shared by the token issuer and the bearer validation setup.
/// </summary>
public static class JwtSetup
{
    // DEV/TEST ONLY — never used in Production (guarded below). Not a real secret. gitleaks:allow
    internal const string DevSigningKey = "dev-only-insecure-jwt-signing-key-change-me-0123456789abcdef";

    public static JwtOptions Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        var options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(options.SigningKey))
        {
            if (environment.IsProduction())
            {
                throw new InvalidOperationException(
                    "Jwt:SigningKey must be configured in Production (provide via secrets manager).");
            }

            options.SigningKey = DevSigningKey;
        }

        return options;
    }
}
