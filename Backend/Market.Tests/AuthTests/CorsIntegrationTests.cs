using System.Net;

namespace Market.Tests.AuthTests;

public sealed class CorsIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CorsIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Preflight_FromAllowedOrigin_ShouldReturnSingleConfiguredCorsHeaderSet()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        using var request = CreatePreflightRequest("http://localhost:4200");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var allowOrigin = AssertSingleHeader(response, "Access-Control-Allow-Origin");
        Assert.Equal("http://localhost:4200", allowOrigin);
        Assert.DoesNotContain(",", allowOrigin);

        var allowMethods = AssertSingleHeader(response, "Access-Control-Allow-Methods");
        Assert.Contains("PATCH", allowMethods, StringComparison.OrdinalIgnoreCase);

        var allowHeaders = AssertSingleHeader(response, "Access-Control-Allow-Headers");
        Assert.Contains("authorization", allowHeaders, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("content-type", allowHeaders, StringComparison.OrdinalIgnoreCase);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
    }

    [Fact]
    public async Task Preflight_FromUnknownOrigin_ShouldNotReturnAllowOrigin()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        using var request = CreatePreflightRequest("https://evil.example");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static HttpRequestMessage CreatePreflightRequest(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/TableReservation/update-status");
        request.Headers.TryAddWithoutValidation("Origin", origin);
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "PATCH");
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Headers", "authorization,content-type");
        return request;
    }

    private static string AssertSingleHeader(HttpResponseMessage response, string headerName)
    {
        Assert.True(response.Headers.TryGetValues(headerName, out var values), $"Missing {headerName} header.");
        var materialized = values.ToArray();
        var value = Assert.Single(materialized);
        return value;
    }
}
