using System.Net;

namespace LegalPlatform.Api.Tests;

/// <summary>
/// Exercises the liveness endpoint through the full middleware pipeline. Uses the shared API factory
/// (one host for the whole assembly) so multiple factories of the same entry point can't interfere.
/// Liveness depends on nothing external, so it passes without Postgres/Redis.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class HealthEndpointTests
{
    private readonly InMemoryApiFactory _factory;

    public HealthEndpointTests(InMemoryApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Liveness_returns_200_and_healthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Liveness_echoes_correlation_id_header()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task Response_carries_security_headers()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
    }
}
