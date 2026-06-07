using LegalPlatform.Modules.Cases.Application;
using LegalPlatform.Modules.Cases.Contracts;
using LegalPlatform.SharedKernel.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LegalPlatform.Modules.Cases.Api;

/// <summary>Minimal-API endpoints for the Cases context (ADR-0005).</summary>
public static class CasesEndpoints
{
    public static IEndpointRouteBuilder MapCasesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cases").WithTags("Cases").RequireAuthorization();

        group.MapPost("", async (
            CreateCaseRequest request,
            CreateCaseHandler handler,
            HttpContext http,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ParseActor(http), ct);
            return result.IsSuccess
                ? Results.Created($"/api/cases/{result.Value.Id}", result.Value)
                : ToProblem(result.Error);
        });

        group.MapGet("", async (GetCasesHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, GetCaseByIdHandler handler, CancellationToken ct) =>
        {
            var dto = await handler.HandleAsync(id, ct);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        });

        return app;
    }

    private static Guid? ParseActor(HttpContext http)
        => Guid.TryParse(http.User.FindFirst("sub")?.Value, out var id) ? id : null;

    private static IResult ToProblem(Error error) => error.Code switch
    {
        "forbidden" => Results.Problem(detail: error.Message, statusCode: StatusCodes.Status403Forbidden),
        "conflict" => Results.Problem(detail: error.Message, statusCode: StatusCodes.Status409Conflict),
        "not_found" => Results.NotFound(error.Message),
        _ => Results.Problem(detail: error.Message, statusCode: StatusCodes.Status400BadRequest),
    };
}
