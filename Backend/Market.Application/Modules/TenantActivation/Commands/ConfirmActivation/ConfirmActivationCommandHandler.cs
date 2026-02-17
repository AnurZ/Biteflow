using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.Tenants;
using Market.Shared.Constants;
using System.Text;

namespace Market.Application.Modules.TenantActivation.Commands.ConfirmActivation
{
    public sealed class ConfirmActivationHandler(
        IAppDbContext db,
        IActivationLinkService links,
        IPasswordHasher<AppUser> hasher,
        UserManager<ApplicationUser> userManager)
        : IRequestHandler<ConfirmActivationCommand, ConfirmActivationResult>
    {
        public async Task<ConfirmActivationResult> Handle(ConfirmActivationCommand r, CancellationToken ct)
        {
            var requestId = await links.ValidateAndConsumeAsync(r.token, ct);

            var e = await db.TenantActivationRequests.FindAsync(new object[] { requestId }, ct)
                     ?? throw new MarketNotFoundException("Request not found");

            if (await db.Tenants.AnyAsync(x => x.Domain == e.Domain, ct) ||
                await db.Restaurants.AnyAsync(x => x.Domain == e.Domain, ct))
            {
                throw new MarketConflictException("Domain already provisioned for another tenant.");
            }

            var tenantId = Guid.NewGuid();
            var restaurantId = Guid.NewGuid();

            var restaurantSlug = BuildRestaurantSlug(e.RestaurantName);
            var adminEmail = await BuildUniqueAdminEmailAsync(restaurantSlug, e.Id, db, userManager, ct);
            var adminPassword = $"{restaurantSlug}firstpassword";

            var identityAdmin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = $"{e.RestaurantName} Admin",
                TenantId = tenantId,
                RestaurantId = restaurantId,
                EmailConfirmed = true,
                IsEnabled = true
            };

            var createIdentity = await userManager.CreateAsync(identityAdmin, adminPassword);
            if (!createIdentity.Succeeded)
            {
                var errors = string.Join(", ", createIdentity.Errors.Select(x => x.Description));
                throw new ValidationException($"Failed to create activation admin user: {errors}");
            }

            var addAdminRole = await userManager.AddToRoleAsync(identityAdmin, RoleNames.Admin);
            if (!addAdminRole.Succeeded)
            {
                await userManager.DeleteAsync(identityAdmin);
                var errors = string.Join(", ", addAdminRole.Errors.Select(x => x.Description));
                throw new ValidationException($"Failed to assign admin role for activation user: {errors}");
            }

            try
            {
                db.Tenants.Add(new Tenant
                {
                    Id = tenantId,
                    Name = e.RestaurantName,
                    Domain = e.Domain,
                    IsActive = true,
                    ActivationRequestId = e.Id,
                    CreatedAtUtc = DateTime.UtcNow
                });

                db.Restaurants.Add(new Restaurant
                {
                    Id = restaurantId,
                    TenantId = tenantId,
                    Name = e.RestaurantName,
                    Domain = e.Domain,
                    Address = e.Address,
                    City = e.City,
                    State = e.State,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                });

                var legacyAdmin = new AppUser
                {
                    TenantId = tenantId,
                    RestaurantId = restaurantId,
                    Email = adminEmail,
                    DisplayName = $"{e.RestaurantName} Admin",
                    IsEmailConfirmed = true,
                    IsLocked = false,
                    IsEnabled = true,
                    TokenVersion = 0
                };

                legacyAdmin.PasswordHash = hasher.HashPassword(legacyAdmin, adminPassword);

                db.Users.Add(legacyAdmin);

                e.MarkActivated(tenantId);
                await db.SaveChangesAsync(ct);
            }
            catch
            {
                await userManager.DeleteAsync(identityAdmin);
                throw;
            }

            return new ConfirmActivationResult(
                TenantId: tenantId,
                RestaurantName: e.RestaurantName,
                AdminUsername: adminEmail,
                AdminPassword: adminPassword);
        }

        private static string BuildRestaurantSlug(string restaurantName)
        {
            var source = (restaurantName ?? string.Empty).Trim().ToLowerInvariant();
            var sb = new StringBuilder(source.Length);

            foreach (var c in source)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
            }

            var slug = sb.ToString();
            return string.IsNullOrWhiteSpace(slug) ? "restaurant" : slug;
        }

        private static async Task<string> BuildUniqueAdminEmailAsync(
            string restaurantSlug,
            int requestId,
            IAppDbContext db,
            UserManager<ApplicationUser> userManager,
            CancellationToken ct)
        {
            var baseLocal = $"{restaurantSlug}.admin";

            for (var attempt = 0; attempt < 50; attempt++)
            {
                var localPart = attempt == 0 ? baseLocal : $"{baseLocal}{requestId + attempt}";
                var email = $"{localPart}@biteflow.com";

                var inLegacy = await db.Users
                    .AnyAsync(x => x.Email.ToLower() == email.ToLower(), ct);

                if (inLegacy) continue;

                var inIdentity = await userManager.FindByEmailAsync(email);
                if (inIdentity != null) continue;

                return email;
            }

            throw new ValidationException("Unable to allocate unique activation admin username.");
        }
    }
}
