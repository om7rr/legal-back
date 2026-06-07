using LegalPlatform.Modules.Identity.Application;
using LegalPlatform.Modules.Identity.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LegalPlatform.Modules.Identity.Api;

/// <summary>Anonymous auth endpoints for the Nafath flow (ADR-0006).</summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app, bool enableSimulator)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth").AllowAnonymous();

        group.MapPost("/nafath/initiate", async (InitiateRequest request, NafathAuthService service, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.NationalId))
            {
                return Results.BadRequest(new { error = "validation", message = "National id is required." });
            }

            return Results.Ok(await service.InitiateAsync(request.NationalId, ct));
        });

        group.MapGet("/nafath/status/{transactionId:guid}", async (Guid transactionId, NafathAuthService service, CancellationToken ct) =>
            Results.Ok(await service.GetStatusAsync(transactionId, ct)));

        group.MapPost("/refresh", async (RefreshRequest request, NafathAuthService service, CancellationToken ct) =>
        {
            var tokens = await service.RefreshAsync(request.RefreshToken, ct);
            return tokens is null ? Results.Unauthorized() : Results.Ok(tokens);
        });

        // Test-environment only: simulates the user approving on their Nafath app.
        if (enableSimulator)
        {
            group.MapPost("/nafath/_simulate-confirm", async (SimulateConfirmRequest request, NafathAuthService service, CancellationToken ct) =>
            {
                var ok = await service.SimulateConfirmAsync(request.TransactionId, request.Accept, ct);
                return ok ? Results.Ok(new { confirmed = request.Accept }) : Results.NotFound();
            });
        }

        return app;
    }
}
