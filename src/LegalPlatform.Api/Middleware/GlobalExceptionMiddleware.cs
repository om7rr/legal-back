using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace LegalPlatform.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions, logs them with the correlation id, and returns a sanitized
/// RFC 7807 ProblemDetails response — never leaking stack traces or internal detail to the caller.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private const string CorrelationHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items.TryGetValue(CorrelationHeader, out var value)
                ? value?.ToString()
                : null;

            _logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);

            if (context.Response.HasStarted)
            {
                throw;
            }

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "The request could not be processed. Reference the correlation id when contacting support.",
                Extensions = { ["correlationId"] = correlationId },
            };

            // Outside Production, surface the cause to aid debugging (never in Production).
            if (!_environment.IsProduction())
            {
                problem.Extensions["exception"] = $"{ex.GetType().FullName}: {ex.Message}";
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
