using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SearchBugs.Api;
using FluentAssertions;

namespace SearchBugs.Api.IntegrationTests;

public class OpenApiPathsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OpenApiPathsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    [Fact]
    public async Task All_Mapped_Groups_Appear_In_OpenApi_Paths()
    {
        var client = _factory.CreateClient();
        using var response = await client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var root = document.RootElement;
        root.TryGetProperty("paths", out var pathsEl).Should().BeTrue();

        var paths = pathsEl.EnumerateObject().Select(p => p.Name).ToHashSet();

        // Expected route prefixes by feature
        var expectedPrefixes = new[]
        {
            "/api/users",
            "/api/roles",
            "/api/repositories",
            "/api/repo",
            "/api/projects",
            "/api/bugs",
            "/api/notifications",
            "/api/audit-logs",
            "/api/analytics",
            "/api/admin",
            "/api/profile",
            "/api/test-notifications"
        };

        foreach (var prefix in expectedPrefixes)
        {
            paths.Any(p => p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Should().BeTrue($"Expected at least one OpenAPI path starting with '{prefix}'");
        }
    }
}


