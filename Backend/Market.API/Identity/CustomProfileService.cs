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

        public CustomProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            if (user == null) return;

            var claims = new List<Claim>
        {
            new("restaurant_id", user.RestaurantId?.ToString() ?? string.Empty),
            new("tenant_id", user.TenantId.ToString()),
            new("display_name", user.DisplayName ?? string.Empty)
        };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

            context.IssuedClaims.AddRange(claims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            context.IsActive = user?.IsEnabled == true;
        }
    }
}
