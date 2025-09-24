using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SearchBugs.Api;
using FluentAssertions;

namespace SearchBugs.Api.IntegrationTests;

public class OpenApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OpenApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    [Fact]
    public async Task OpenApi_v1_json_is_available()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"openapi\"");
        json.Should().Contain("\"paths\"");
    }
}
