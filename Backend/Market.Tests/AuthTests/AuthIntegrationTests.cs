using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Market.Domain.Entities.IdentityV2;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

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

    [Fact]
    public async Task IdentityServerToken_InvalidCredentials_ShouldUseSamePublicError_ForMissingAndWrongPassword()
    {
        var username = $"wrong-password-{Guid.NewGuid():N}@example.test";
        await CreateIdentityUserAsync(username, "correct-password");
        var client = _factory.CreateClient();

        var missingUser = await RequestPasswordTokenAsync(client, $"missing-{Guid.NewGuid():N}@example.test", "any-password");
        var wrongPassword = await RequestPasswordTokenAsync(client, username, "wrong-password");

        Assert.Equal(HttpStatusCode.BadRequest, missingUser.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, wrongPassword.StatusCode);
        Assert.Equal("invalid_grant", missingUser.Error);
        Assert.Equal(missingUser.Error, wrongPassword.Error);
        Assert.Equal("Neispravni podaci za prijavu.", missingUser.ErrorDescription);
        Assert.Equal(missingUser.ErrorDescription, wrongPassword.ErrorDescription);
    }

    [Fact]
    public async Task IdentityServerToken_LockedUser_ShouldUseGenericPublicError()
    {
        var username = $"locked-{Guid.NewGuid():N}@example.test";
        await CreateIdentityUserAsync(username, "correct-password", lockedOut: true);

        var client = _factory.CreateClient();
        var locked = await RequestPasswordTokenAsync(client, username, "correct-password");

        Assert.Equal(HttpStatusCode.BadRequest, locked.StatusCode);
        Assert.Equal("invalid_grant", locked.Error);
        Assert.Equal("Neispravni podaci za prijavu.", locked.ErrorDescription);
    }

    private async Task<ApplicationUser> CreateIdentityUserAsync(
        string username,
        string password,
        bool lockedOut = false)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = username,
            Email = username,
            EmailConfirmed = true,
            DisplayName = username,
            TenantId = SeedConstants.DefaultTenantId,
            RestaurantId = SeedConstants.DefaultRestaurantId,
            IsEnabled = true,
            LockoutEnabled = true,
            LockoutEnd = lockedOut ? DateTimeOffset.UtcNow.AddMinutes(5) : null
        };

        var result = await userManager.CreateAsync(user, password);
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(x => x.Description)));

        return user;
    }

    private static async Task<TokenErrorResponse> RequestPasswordTokenAsync(HttpClient client, string username, string password)
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
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return new TokenErrorResponse(
            response.StatusCode,
            payload.RootElement.GetProperty("error").GetString(),
            payload.RootElement.TryGetProperty("error_description", out var errorDescription)
                ? errorDescription.GetString()
                : null);
    }

    private sealed record TokenErrorResponse(
        HttpStatusCode StatusCode,
        string? Error,
        string? ErrorDescription);
}
