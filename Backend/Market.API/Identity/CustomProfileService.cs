using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Market.Application.Abstractions;
using Market.Domain.Entities.IdentityV2;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Market.API.Identity
{
    public sealed class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppDbContext _db;
        private readonly ILogger<CustomProfileService> _logger;

        public CustomProfileService(
            UserManager<ApplicationUser> userManager,
            IAppDbContext db,
            ILogger<CustomProfileService> logger)
        {
            _userManager = userManager;
            _db = db;
            _logger = logger;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            if (user == null) return;

            _logger.LogInformation("Issuing profile claims for user {UserId} ({Email}) with display name '{DisplayName}'", user.Id, user.Email, user.DisplayName);

            var effectiveTenantId = user.TenantId == Guid.Empty
                ? SeedConstants.DefaultTenantId
                : user.TenantId;
            var tenantName = await ResolveTenantNameAsync(effectiveTenantId, CancellationToken.None);
            var position = await ResolvePositionAsync(user, CancellationToken.None);

            var claims = new List<Claim>
            {
                new("restaurant_id", user.RestaurantId?.ToString() ?? string.Empty),
                new("tenant_id", effectiveTenantId.ToString()),
                new("display_name", user.DisplayName ?? string.Empty),
                new(JwtClaimTypes.Name, user.DisplayName ?? user.UserName ?? string.Empty),
                new(JwtClaimTypes.PreferredUserName, user.UserName ?? string.Empty)
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

            if (!string.IsNullOrWhiteSpace(tenantName))
            {
                claims.Add(new Claim("tenant_name", tenantName!));
            }

            if (!string.IsNullOrWhiteSpace(position))
            {
                claims.Add(new Claim("position", position));
            }

            context.IssuedClaims.AddRange(claims);
            context.AddRequestedClaims(claims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            context.IsActive = user?.IsEnabled == true;
        }

        private async Task<string?> ResolveTenantNameAsync(Guid tenantId, CancellationToken ct)
        {
            if (tenantId == Guid.Empty || tenantId == SeedConstants.DefaultTenantId)
            {
                return null;
            }

            return await _db.TenantActivationRequests
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => x.RestaurantName)
                .FirstOrDefaultAsync(ct)
                ?? await _db.Tenants
                .AsNoTracking()
                .Where(x => x.Id == tenantId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(ct);
        }

        private async Task<string?> ResolvePositionAsync(ApplicationUser user, CancellationToken ct)
        {
            var profile = await _db.EmployeeProfiles
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id, ct);

            if (profile != null && !string.IsNullOrWhiteSpace(profile.Position))
            {
                return profile.Position;
            }

            var userNameOrEmail = (user.UserName ?? user.Email ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(userNameOrEmail))
            {
                return null;
            }

            var legacyUserId = await _db.Users
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(x => x.Email.ToLower() == userNameOrEmail)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(ct);

            if (legacyUserId == 0)
            {
                return null;
            }

            return await _db.EmployeeProfiles
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(x => x.AppUserId == legacyUserId)
                .Select(x => x.Position)
                .FirstOrDefaultAsync(ct);
        }
    }
}
