using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LegalPlatform.Api.Tests;

/// <summary>
/// End-to-end Cases endpoint tests through the real pipeline (InMemory DB), now authenticated via the
/// mock Nafath flow. Tenant comes from the JWT, so isolation is driven by which seeded user logs in.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class CasesEndpointsTests
{
    private readonly InMemoryApiFactory _factory;

    public CasesEndpointsTests(InMemoryApiFactory factory) => _factory = factory;

    private async Task<HttpClient> AuthedClientAsync(string nationalId)
    {
        var token = await AuthTestHelper.GetAccessTokenAsync(_factory, nationalId);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static object NewCase(string number, string title) => new
    {
        caseNumber = number,
        title,
        type = "tijari",
        court = "المحكمة التجارية — الرياض",
        clientId = Guid.NewGuid(),
        leadLawyerId = Guid.NewGuid(),
    };

    private sealed record CaseListItemDto(Guid Id, string CaseNumber, string Title, string Type, string Court, string Status);
    private sealed record CreateCaseResponseDto(Guid Id, string CaseNumber);

    [Fact]
    public async Task Unauthenticated_request_is_401()
    {
        var client = _factory.CreateClient(); // no token
        var res = await client.PostAsJsonAsync("/api/cases", NewCase("C-AUTH", "no token"));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Create_is_visible_to_same_tenant_and_hidden_from_another()
    {
        var firmA = await AuthedClientAsync(AuthTestHelper.FirmAAdmin);
        var firmB = await AuthedClientAsync(AuthTestHelper.FirmBAdmin);

        var created = await firmA.PostAsJsonAsync("/api/cases", NewCase("C-VIS-001", "قضية تجارية"));
        Assert.True(created.StatusCode == HttpStatusCode.Created, await created.Content.ReadAsStringAsync());

        var listA = await firmA.GetFromJsonAsync<List<CaseListItemDto>>("/api/cases");
        Assert.Contains(listA!, c => c.CaseNumber == "C-VIS-001");

        var listB = await firmB.GetFromJsonAsync<List<CaseListItemDto>>("/api/cases");
        Assert.DoesNotContain(listB!, c => c.CaseNumber == "C-VIS-001");
    }

    [Fact]
    public async Task Duplicate_case_number_within_tenant_conflicts()
    {
        var firmA = await AuthedClientAsync(AuthTestHelper.FirmAAdmin);

        var first = await firmA.PostAsJsonAsync("/api/cases", NewCase("C-DUP-1", "first"));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await firmA.PostAsJsonAsync("/api/cases", NewCase("C-DUP-1", "second"));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Get_by_id_is_404_for_another_tenant()
    {
        var firmA = await AuthedClientAsync(AuthTestHelper.FirmAAdmin);
        var firmB = await AuthedClientAsync(AuthTestHelper.FirmBAdmin);

        var created = await firmA.PostAsJsonAsync("/api/cases", NewCase("C-OWN-777", "owned by A"));
        var body = await created.Content.ReadFromJsonAsync<CreateCaseResponseDto>();
        var path = $"/api/cases/{body!.Id}";

        Assert.Equal(HttpStatusCode.OK, (await firmA.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await firmB.GetAsync(path)).StatusCode);
    }
}
