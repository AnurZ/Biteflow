using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.Tenants;
using Market.Shared.Constants;
using Market.Shared.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Market.Application.Modules.TenantActivation.Commands.ConfirmActivation
{
    public sealed class ConfirmActivationHandler(
        IAppDbContext db,
        IActivationLinkService links,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IOptions<ActivationLinkOptions> activationLinkOptions)
        : IRequestHandler<ConfirmActivationCommand, ConfirmActivationResult>
    {
        public async Task<ConfirmActivationResult> Handle(ConfirmActivationCommand r, CancellationToken ct)
        {
            var requestId = await links.ValidateAndConsumeAsync(r.token, ct);

            var e = await db.TenantActivationRequests
                // Activation confirmation consumes a pre-tenant request before tenant context exists.
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == requestId, ct)
                ?? throw new MarketNotFoundException("Request not found");

            if (await db.Tenants
                    // Activation confirmation must validate domain uniqueness across all tenants.
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.Domain == e.Domain, ct) ||
                await db.Restaurants
                    // Activation confirmation must validate restaurant domains across all tenants.
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.Domain == e.Domain, ct))
            {
                throw new MarketConflictException("Domain already provisioned for another tenant.");
            }

            var tenantId = Guid.NewGuid();
            var restaurantId = Guid.NewGuid();

            var restaurantSlug = BuildRestaurantSlug(e.RestaurantName);
            var adminEmail = await BuildUniqueAdminEmailAsync(restaurantSlug, e.Id, db, userManager, ct);
            var adminPassword = GenerateTemporaryPassword();

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

                e.MarkActivated(tenantId);
                await db.SaveChangesAsync(ct);
            }
            catch
            {
                await userManager.DeleteAsync(identityAdmin);
                throw;
            }

            var passwordSetupToken = await userManager.GeneratePasswordResetTokenAsync(identityAdmin);
            var passwordSetupLink = BuildPasswordSetupLink(
                activationLinkOptions.Value.BaseUrl,
                identityAdmin.Id,
                passwordSetupToken);

            await SendOnboardingEmailSafeAsync(
                emailService,
                e.OwnerEmail,
                e.OwnerFullName,
                e.RestaurantName,
                adminEmail,
                passwordSetupLink,
                ct);

            return new ConfirmActivationResult(
                TenantId: tenantId,
                AdminUsername: adminEmail);
        }

        private static async Task SendOnboardingEmailSafeAsync(
            IEmailService emailService,
            string ownerEmail,
            string ownerFullName,
            string restaurantName,
            string adminUsername,
            string passwordSetupLink,
            CancellationToken ct)
        {
            var subject = "Biteflow - Your Restaurant Has Been Approved";
            var body = $"""
Hello {ownerFullName},

Welcome to Biteflow. Your restaurant "{restaurantName}" has been approved and your tenant account is ready.

Use the link below to set your admin password. The link is short-lived and can only be used once.
Username: {adminUsername}
Password setup link: {passwordSetupLink}

Best regards,
Biteflow Team
""";

            try
            {
                await emailService.SendAsync(ownerEmail, subject, body, ct);
            }
            catch
            {
                // Email sending is best-effort and must not block activation.
            }
        }

        private static string GenerateTemporaryPassword()
        {
            var buffer = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(buffer)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_') + "Aa1!";
        }

        private static string BuildPasswordSetupLink(string baseUrl, Guid userId, string token)
        {
            var root = (string.IsNullOrWhiteSpace(baseUrl) ? "https://localhost:4200" : baseUrl).TrimEnd('/');
            return $"{root}/activate/set-password?userId={userId:D}&token={WebUtility.UrlEncode(token)}";
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

                var inIdentity = await userManager.FindByEmailAsync(email);
                if (inIdentity != null) continue;

                return email;
            }

            throw new ValidationException("Unable to allocate unique activation admin username.");
        }
    }
}
