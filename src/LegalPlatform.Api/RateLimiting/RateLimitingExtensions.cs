using System.Threading.RateLimiting;

namespace LegalPlatform.Api.RateLimiting;

/// <summary>
/// Configures the API rate limiter, partitioned per tenant (falling back to client IP). This is an
/// in-process limiter; for horizontal scale it is replaced by a Redis-backed distributed limiter so
/// limits hold across instances (see docs/api/rate-limits.md).
/// </summary>
public static class RateLimitingExtensions
{
    public static IServiceCollection AddPlatformRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                return ValueTask.CompletedTask;
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var tenant = httpContext.Request.Headers["X-Tenant-Id"].ToString();
                var partitionKey = !string.IsNullOrWhiteSpace(tenant)
                    ? $"tenant:{tenant}"
                    : $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                });
            });
        });

        return services;
    }
}
