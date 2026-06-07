using System.Net.Http.Json;

namespace LegalPlatform.Api.Tests;

/// <summary>Runs the mock Nafath flow (initiate → simulate-confirm → status) to obtain a JWT in tests.</summary>
internal static class AuthTestHelper
{
    // Seeded test identities (see IdentitySeeder).
    public const string FirmAAdmin = "1111111111";
    public const string FirmBAdmin = "3333333333";

    private sealed record InitiateDto(Guid TransactionId, string Number, string Message);
    private sealed record StatusDto(string Status, string? AccessToken, string? RefreshToken, int? ExpiresInSeconds, string? Message);

    public static async Task<string> GetAccessTokenAsync(InMemoryApiFactory factory, string nationalId)
    {
        var client = factory.CreateClient();

        var initResponse = await client.PostAsJsonAsync("/api/auth/nafath/initiate", new { nationalId });
        var init = await initResponse.Content.ReadFromJsonAsync<InitiateDto>();

        await client.PostAsJsonAsync(
            "/api/auth/nafath/_simulate-confirm",
            new { transactionId = init!.TransactionId, accept = true });

        var statusResponse = await client.GetAsync($"/api/auth/nafath/status/{init.TransactionId}");
        var status = await statusResponse.Content.ReadFromJsonAsync<StatusDto>();

        return status!.AccessToken!;
    }
}
