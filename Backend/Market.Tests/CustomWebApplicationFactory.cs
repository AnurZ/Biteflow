using System.Net.Http.Headers;
using System.Text.Json;

namespace Market.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<Program>
{
    private static string? _cachedToken;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
    }

    public async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var client = CreateClient();
        if (string.IsNullOrEmpty(_cachedToken))
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "biteflow-tests",
                ["username"] = "string",
                ["password"] = "string",
                ["scope"] = "openid profile email roles biteflow.api"
            });

            var response = await client.PostAsync("connect/token", content);
            response.EnsureSuccessStatusCode();

            using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            _cachedToken = payload.RootElement.GetProperty("access_token").GetString();
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _cachedToken);
        return client;
    }
}
