using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Market.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Market.Tests.ProductCategoryTests.IntegrationTests;

public class ProductCategoryIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductCategoryIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.GetAuthenticatedClientAsync().Result;
    }

    [Fact]
    public async Task Post_CreateProductCategory_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/ProductCategories", new
        {
            Name = "Integration Test Category"
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("id"));

        var categoryId = result["id"];
        Assert.NotEqual(0, categoryId);
    }

    [Fact]
    public async Task DeleteProductCategory_WithAuthenticatedGuidUser_ShouldSoftDeleteCategory()
    {
        var categoryId = await CreateCategoryAsync("Delete Integration Test Category");

        var response = await _client.DeleteAsync($"/ProductCategories/{categoryId}");

        response.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var category = await db.ProductCategories
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Id == categoryId);

        Assert.True(category.IsDeleted);
    }

    [Fact]
    public async Task DeleteProductCategory_WithInvalidUserIdClaim_ShouldFailAsUnauthenticated()
    {
        var categoryId = await CreateCategoryAsync("Invalid User Claim Delete Test Category");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", BuildUnsignedJwt("not-a-guid"));

        var response = await client.DeleteAsync($"/ProductCategories/{categoryId}");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("Korisnik nije autentifikovan", body);
    }

    private async Task<int> CreateCategoryAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/ProductCategories", new { Name = $"{name} {Guid.NewGuid():N}" });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        Assert.NotNull(result);

        return result!["id"];
    }

    private static string BuildUnsignedJwt(string userId)
    {
        var header = new Dictionary<string, object?>
        {
            ["alg"] = "none",
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object?>
        {
            ["sub"] = userId,
            ["aud"] = "biteflow.api",
            ["exp"] = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
            ["role"] = RoleNames.Admin,
            ["tenant_id"] = SeedConstants.DefaultTenantId.ToString(),
            ["restaurant_id"] = SeedConstants.DefaultRestaurantId.ToString()
        };

        return $"{Base64Url(header)}.{Base64Url(payload)}.";
    }

    private static string Base64Url(object value)
    {
        var json = JsonSerializer.Serialize(value);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
