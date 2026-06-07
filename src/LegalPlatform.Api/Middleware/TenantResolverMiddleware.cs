using LegalPlatform.Api.Multitenancy;
using Serilog.Context;

namespace LegalPlatform.Api.Middleware;

/// <summary>
/// Resolves the tenant for the current request from the authenticated principal's verified
/// <c>tenant_id</c> claim (issued by the auth flow — ADR-0006) and sets the scoped
/// <see cref="TenantContext"/>. Runs after authentication. The id is also pushed onto the log context.
/// </summary>
public sealed class TenantResolverMiddleware
{
    private const string TenantClaim = "tenant_id";
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var raw = context.User.FindFirst(TenantClaim)?.Value;
            if (Guid.TryParse(raw, out var tenantId))
            {
                tenantContext.SetTenant(tenantId);
            }
        }

        using (LogContext.PushProperty("TenantId", tenantContext.HasTenant ? tenantContext.TenantId : "none"))
        {
            await _next(context);
        }
    }
}
