using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LegalPlatform.Api.Health;

/// <summary>Writes a compact JSON health report. Never includes exception detail (no info leak).</summary>
public static class HealthResponseWriter
{
    public static Task WriteJson(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
            }),
        };

        return context.Response.WriteAsJsonAsync(payload);
    }
}
