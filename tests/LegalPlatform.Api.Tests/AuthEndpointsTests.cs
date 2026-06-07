using System.Net;
using System.Net.Http.Json;

namespace LegalPlatform.Api.Tests;

/// <summary>Exercises the mock Nafath flow end-to-end (ADR-0006).</summary>
[Collection(ApiCollection.Name)]
public sealed class AuthEndpointsTests
{
    private readonly InMemoryApiFactory _factory;

    public AuthEndpointsTests(InMemoryApiFactory factory) => _factory = factory;

    private sealed record InitiateDto(Guid TransactionId, string Number, string Message);
    private sealed record StatusDto(string Status, string? AccessToken, string? RefreshToken, int? ExpiresInSeconds, string? Message);
    private sealed record TokenDto(string AccessToken, string RefreshToken, int ExpiresInSeconds);

    private async Task<InitiateDto> InitiateAsync(HttpClient client, string nationalId)
    {
        var res = await client.PostAsJsonAsync("/api/auth/nafath/initiate", new { nationalId });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        return (await res.Content.ReadFromJsonAsync<InitiateDto>())!;
    }

    [Fact]
    public async Task Full_flow_issues_a_token_for_a_known_user()
    {
        var client = _factory.CreateClient();
        var init = await InitiateAsync(client, AuthTestHelper.FirmAAdmin);
        Assert.Matches("^[0-9]{2}$", init.Number);

        // Pending before confirmation.
        var pending = await (await client.GetAsync($"/api/auth/nafath/status/{init.TransactionId}"))
            .Content.ReadFromJsonAsync<StatusDto>();
        Assert.Equal("Pending", pending!.Status);

        await client.PostAsJsonAsync("/api/auth/nafath/_simulate-confirm", new { transactionId = init.TransactionId, accept = true });

        var completed = await (await client.GetAsync($"/api/auth/nafath/status/{init.TransactionId}"))
            .Content.ReadFromJsonAsync<StatusDto>();
        Assert.Equal("Completed", completed!.Status);
        Assert.False(string.IsNullOrWhiteSpace(completed.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(completed.RefreshToken));
    }

    [Fact]
    public async Task Verified_but_unprovisioned_user_gets_no_token()
    {
        var client = _factory.CreateClient();
        var init = await InitiateAsync(client, "9999999999"); // not seeded
        await client.PostAsJsonAsync("/api/auth/nafath/_simulate-confirm", new { transactionId = init.TransactionId, accept = true });

        var status = await (await client.GetAsync($"/api/auth/nafath/status/{init.TransactionId}"))
            .Content.ReadFromJsonAsync<StatusDto>();
        Assert.Equal("Completed", status!.Status);
        Assert.Null(status.AccessToken);
        Assert.False(string.IsNullOrWhiteSpace(status.Message));
    }

    [Fact]
    public async Task Rejected_confirmation_yields_rejected_status()
    {
        var client = _factory.CreateClient();
        var init = await InitiateAsync(client, AuthTestHelper.FirmAAdmin);
        await client.PostAsJsonAsync("/api/auth/nafath/_simulate-confirm", new { transactionId = init.TransactionId, accept = false });

        var status = await (await client.GetAsync($"/api/auth/nafath/status/{init.TransactionId}"))
            .Content.ReadFromJsonAsync<StatusDto>();
        Assert.Equal("Rejected", status!.Status);
        Assert.Null(status.AccessToken);
    }

    [Fact]
    public async Task Refresh_returns_new_tokens()
    {
        var client = _factory.CreateClient();
        var init = await InitiateAsync(client, AuthTestHelper.FirmBAdmin);
        await client.PostAsJsonAsync("/api/auth/nafath/_simulate-confirm", new { transactionId = init.TransactionId, accept = true });
        var status = await (await client.GetAsync($"/api/auth/nafath/status/{init.TransactionId}"))
            .Content.ReadFromJsonAsync<StatusDto>();

        var refreshed = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = status!.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);
        var tokens = await refreshed.Content.ReadFromJsonAsync<TokenDto>();
        Assert.False(string.IsNullOrWhiteSpace(tokens!.AccessToken));
        Assert.NotEqual(status.RefreshToken, tokens.RefreshToken); // rotated
    }

    [Fact]
    public async Task Refresh_with_unknown_token_is_401()
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = "not-a-real-token" });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
