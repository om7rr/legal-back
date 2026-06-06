namespace LegalPlatform.Api.Middleware;

/// <summary>Adds baseline security response headers to every response.</summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "no-referrer";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";

        // Swagger UI (development-only) is an HTML/JS app and needs to load its own assets,
        // so it can't run under the API's lock-everything CSP. Exempt that path only.
        var isSwaggerUi = context.Request.Path.StartsWithSegments("/swagger");
        if (!isSwaggerUi)
        {
            headers["X-Frame-Options"] = "DENY";
            // API serves JSON only; lock down what a response may load/embed.
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        }

        return _next(context);
    }
}
