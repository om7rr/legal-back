using LegalPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LegalPlatform.Api.Health;

/// <summary>Readiness check: can the platform reach its PostgreSQL database?</summary>
public sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly AppDbContext _db;

    public PostgresHealthCheck(AppDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _db.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("PostgreSQL reachable.")
                : HealthCheckResult.Unhealthy("PostgreSQL not reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL check failed.", ex);
        }
    }
}
