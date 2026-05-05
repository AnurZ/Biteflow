using System.Net;
using System.Net.Http.Json;

namespace Market.Tests.AuthTests;

public sealed class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AuthIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LegacyLoginEndpoint_ShouldNotExist()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("api/auth/login", JsonContent.Create(new
        {
            Email = "test",
            Password = "test123"
        }));

        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.NotFound, HttpStatusCode.Unauthorized });
    }

    [Fact]
    public async Task IdentityServerToken_ShouldAuthorizeProtectedEndpoint()
    {
        var client = await _factory.GetAuthenticatedClientAsync();

        var response = await client.GetAsync("/ProductCategories");

        response.EnsureSuccessStatusCode();
    }
}
