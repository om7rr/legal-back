using System.Net;
using System.Net.Http.Json;

namespace LegalPlatform.Api.Tests;

/// <summary>
/// End-to-end Cases endpoint tests through the real pipeline (InMemory DB). Each test uses fresh
/// tenant ids so the shared store can't bleed between tests; tenant scoping does the isolation.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class CasesEndpointsTests
{
    private readonly InMemoryApiFactory _factory;

    public CasesEndpointsTests(InMemoryApiFactory factory) => _factory = factory;

    private HttpClient ClientFor(string tenantId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
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

    [Fact]
    public async Task Create_then_list_is_scoped_to_the_creating_tenant()
    {
        var tenantA = Guid.NewGuid().ToString();
        var tenantB = Guid.NewGuid().ToString();

        var created = await ClientFor(tenantA).PostAsJsonAsync("/api/cases", NewCase("C-001", "قضية تجارية"));
        Assert.True(created.StatusCode == HttpStatusCode.Created, await created.Content.ReadAsStringAsync());

        var listA = await ClientFor(tenantA).GetFromJsonAsync<List<CaseListItemDto>>("/api/cases");
        Assert.NotNull(listA);
        Assert.Single(listA!);
        Assert.Equal("C-001", listA![0].CaseNumber);

        var listB = await ClientFor(tenantB).GetFromJsonAsync<List<CaseListItemDto>>("/api/cases");
        Assert.NotNull(listB);
        Assert.Empty(listB!);
    }

    [Fact]
    public async Task Create_without_tenant_header_is_forbidden()
    {
        var client = _factory.CreateClient(); // no X-Tenant-Id
        var res = await client.PostAsJsonAsync("/api/cases", NewCase("C-NOH", "no tenant"));
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Duplicate_case_number_within_tenant_conflicts()
    {
        var tenant = Guid.NewGuid().ToString();
        var first = await ClientFor(tenant).PostAsJsonAsync("/api/cases", NewCase("DUP-1", "first"));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await ClientFor(tenant).PostAsJsonAsync("/api/cases", NewCase("DUP-1", "second"));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Get_by_id_is_404_for_another_tenant()
    {
        var tenantA = Guid.NewGuid().ToString();
        var tenantB = Guid.NewGuid().ToString();

        var created = await ClientFor(tenantA).PostAsJsonAsync("/api/cases", NewCase("C-777", "owned by A"));
        var body = await created.Content.ReadFromJsonAsync<CreateCaseResponseDto>();
        Assert.NotNull(body);
        var path = $"/api/cases/{body!.Id}";

        Assert.Equal(HttpStatusCode.OK, (await ClientFor(tenantA).GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ClientFor(tenantB).GetAsync(path)).StatusCode);
    }

    private sealed record CaseListItemDto(Guid Id, string CaseNumber, string Title, string Type, string Court, string Status);

    private sealed record CreateCaseResponseDto(Guid Id, string CaseNumber);
}
