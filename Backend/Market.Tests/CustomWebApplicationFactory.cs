using System.Net.Http.Headers;
using System.Text.Json;

namespace Market.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<Program>
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> CachedTokens = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
    }

    public async Task<HttpClient> GetAuthenticatedClientAsync()
        => await GetAuthenticatedClientAsync("string", "string");

    public async Task<HttpClient> GetAuthenticatedClientAsync(string username, string password)
    {
        var client = CreateClient();
        var cacheKey = $"{username}:{password}";
        if (!CachedTokens.TryGetValue(cacheKey, out var token))
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "biteflow-tests",
                ["username"] = username,
                ["password"] = password,
                ["scope"] = "openid profile email roles biteflow.api"
            });

            var response = await client.PostAsync("connect/token", content);
            response.EnsureSuccessStatusCode();

            using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            token = payload.RootElement.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Token endpoint returned an empty access token.");
            CachedTokens[cacheKey] = token;
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
