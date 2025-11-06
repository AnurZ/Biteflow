using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Market.Domain.Entities.IdentityV2;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Market.API.Identity
{
    public sealed class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CustomProfileService> _logger;

        public CustomProfileService(UserManager<ApplicationUser> userManager, ILogger<CustomProfileService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            if (user == null) return;

            _logger.LogInformation("Issuing profile claims for user {UserId} ({Email}) with display name '{DisplayName}'", user.Id, user.Email, user.DisplayName);

            var claims = new List<Claim>
            {
                new("restaurant_id", user.RestaurantId?.ToString() ?? string.Empty),
                new("tenant_id", user.TenantId.ToString()),
                new("display_name", user.DisplayName ?? string.Empty),
                new(JwtClaimTypes.Name, user.DisplayName ?? user.UserName ?? string.Empty),
                new(JwtClaimTypes.PreferredUserName, user.UserName ?? string.Empty)
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

            context.IssuedClaims.AddRange(claims);
            context.AddRequestedClaims(claims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            context.IsActive = user?.IsEnabled == true;
        }
    }
}
