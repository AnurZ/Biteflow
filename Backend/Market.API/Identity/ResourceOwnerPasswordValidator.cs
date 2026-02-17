using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Market.Domain.Entities.Identity;
using Market.Domain.Entities.IdentityV2;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Market.API.Identity;

public sealed class ResourceOwnerPasswordValidator(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<ResourceOwnerPasswordValidator> logger) : IResourceOwnerPasswordValidator
{
    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        var username = (context.UserName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Username required.");
            return;
        }

        var user =
            await userManager.FindByNameAsync(username) ??
            await userManager.FindByEmailAsync(username) ??
            await userManager.FindByEmailAsync($"{username}@legacy.local");

        if (user is null)
        {
            logger.LogWarning("Resource owner password flow failed. User '{Username}' not found.", username);
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid credentials.");
            return;
        }

        var signIn = await signInManager.CheckPasswordSignInAsync(user, context.Password, lockoutOnFailure: true);
        if (!signIn.Succeeded)
        {
            if (signIn.IsLockedOut)
            {
                logger.LogWarning("User '{Username}' is locked out.", user.UserName);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Account locked. Try again later.");
            }
            else
            {
                logger.LogWarning("Invalid credentials supplied for '{Username}'.", user.UserName);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid credentials.");
            }
            return;
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = BuildClaims(user, roles);

        logger.LogInformation("Resource owner password flow claims for '{UserName}': {Claims}",
            user.UserName,
            string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));

        context.Result = new GrantValidationResult(
            subject: user.Id.ToString(),
            authenticationMethod: "password",
            claims: claims);

        logger.LogInformation("Resource owner password flow succeeded for '{UserName}'.", user.UserName);
    }

    private static IEnumerable<Claim> BuildClaims(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtClaimTypes.Subject, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtClaimTypes.Name, user.DisplayName ?? user.UserName ?? string.Empty),
            new Claim(JwtClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("display_name", user.DisplayName ?? user.UserName ?? string.Empty),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim("restaurant_id", user.RestaurantId?.ToString() ?? Guid.Empty.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }
}
