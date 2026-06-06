using LegalPlatform.Api.Multitenancy;
using Serilog.Context;

namespace LegalPlatform.Api.Middleware;

/// <summary>
/// Resolves the tenant for the current request and sets the scoped <see cref="TenantContext"/>.
/// For now the tenant is read from the <c>X-Tenant-Id</c> header; once Nafath/JWT auth lands it will
/// come from a verified token claim. The id is also pushed onto the log context.
/// </summary>
public sealed class TenantResolverMiddleware
{
    private const string HeaderName = "X-Tenant-Id";
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var raw)
            && Guid.TryParse(raw.ToString(), out var tenantId))
        {
            tenantContext.SetTenant(tenantId);
        }

        using (LogContext.PushProperty("TenantId", tenantContext.HasTenant ? tenantContext.TenantId : "none"))
        {
            await _next(context);
        }
    }
}
