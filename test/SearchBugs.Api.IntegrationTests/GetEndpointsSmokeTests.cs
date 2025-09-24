using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using SearchBugs.Api;
using FluentAssertions;

namespace SearchBugs.Api.IntegrationTests;

public class GetEndpointsSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GetEndpointsSmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    [Theory]
    [InlineData("/api/users/")]
    [InlineData("/api/roles")] 
    [InlineData("/api/repositories")] 
    [InlineData("/api/projects")] 
    [InlineData("/api/bugs")] 
    [InlineData("/api/notifications")] 
    [InlineData("/api/profile/")] 
    [InlineData("/openapi/v1.json")] 
    public async Task Get_should_not_return_404(string path)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(path);
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }
}
