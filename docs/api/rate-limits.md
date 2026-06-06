# Rate limits

Configured in `LegalPlatform.Api/RateLimiting/RateLimitingExtensions.cs`.

- **Partition:** per **tenant** (`X-Tenant-Id`), falling back to client **IP** when no tenant.
- **Default:** fixed window, **100 requests / minute** per partition, no queue.
- **Rejection:** HTTP **429** with **`Retry-After: 60`**.
- **Exemptions:** health endpoints (`/health`, `/health/ready`) are not rate limited.

## Scaling note (grounded)
The current limiter is **in-process** (per instance). For horizontal scale, replace it with a
**Redis-backed distributed limiter** so limits hold across instances (Redis is already provisioned
for caching). Tracked as a follow-up; tiers will become configurable per tenant plan.
