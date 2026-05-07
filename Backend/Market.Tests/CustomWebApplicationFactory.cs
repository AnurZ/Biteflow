using System.Net.Http.Headers;
using Market.Application.Abstractions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Market.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<Program>
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> CachedTokens = new();
    private static readonly System.Collections.Concurrent.ConcurrentQueue<SentEmail> SentEmails = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.ConfigureTestServices(services =>
        {
            var descriptors = services
                .Where(x => x.ServiceType == typeof(IEmailService))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IEmailService, CapturingEmailService>();
        });
    }

    public static void ClearSentEmails()
    {
        while (SentEmails.TryDequeue(out _))
        {
        }
    }

    public static IReadOnlyList<SentEmail> GetSentEmails()
        => SentEmails.ToArray();

    public async Task<HttpClient> GetAuthenticatedClientAsync()
        => await GetAuthenticatedClientAsync("string", "StringUser1!");

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

    public sealed record SentEmail(string ToEmail, string Subject, string Body);

    private sealed class CapturingEmailService : IEmailService
    {
        public Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
        {
            SentEmails.Enqueue(new SentEmail(toEmail, subject, body));
            return Task.CompletedTask;
        }
    }
}
