using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace LegalPlatform.Api.Health;

/// <summary>Readiness check: is the Redis cache reachable?</summary>
public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis) => _redis = redis;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _redis.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy("Redis reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis not reachable.", ex);
        }
    }
}
