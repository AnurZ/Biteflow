using System;
using System.Security.Claims;
using Market.Application.Abstractions;
using Market.Shared.Constants;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Market.API.Hubs
{
    public static class OrdersHubEvents
    {
        public const string OrderCreated = "OrderCreated";
        public const string OrderStatusChanged = "OrderStatusChanged";
    }

    public static class OrdersHubGroups
    {
        public static string Kitchen(string? tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return "kitchen";
            }

            if (Guid.TryParse(tenantId, out var parsed) &&
                (parsed == Guid.Empty || parsed == SeedConstants.DefaultTenantId))
            {
                return "kitchen";
            }

            return $"tenant:{tenantId}:kitchen";
        }

        public static string Waiter(string? tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return "waiter";
            }

            if (Guid.TryParse(tenantId, out var parsed) &&
                (parsed == Guid.Empty || parsed == SeedConstants.DefaultTenantId))
            {
                return "waiter";
            }

            return $"tenant:{tenantId}:waiter";
        }

        public static string Kitchen(Guid tenantId)
        {
            if (tenantId == Guid.Empty || tenantId == SeedConstants.DefaultTenantId)
            {
                return "kitchen";
            }

            return $"tenant:{tenantId}:kitchen";
        }

        public static string Waiter(Guid tenantId)
        {
            if (tenantId == Guid.Empty || tenantId == SeedConstants.DefaultTenantId)
            {
                return "waiter";
            }

            return $"tenant:{tenantId}:waiter";
        }
    }

    [Authorize(Policy = PolicyNames.StaffMember)]
    public sealed class OrdersHub : Hub
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<OrdersHub> _logger;

        public OrdersHub(IAppDbContext db, ILogger<OrdersHub> logger)
        {
            _db = db;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
            var groups = await ResolveGroupsAsync(Context.User, tenantId, Context.ConnectionAborted);

            foreach (var group in groups)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
            }

            _logger.LogInformation("OrdersHub connected {ConnectionId}. Groups={Groups}", Context.ConnectionId, string.Join(",", groups));
            await base.OnConnectedAsync();
        }

        private async Task<IReadOnlyCollection<string>> ResolveGroupsAsync(
            ClaimsPrincipal? user,
            string? tenantId,
            CancellationToken ct)
        {
            var groups = new List<string>();
            if (user == null)
            {
                return groups;
            }

            var position = await GetPositionAsync(user, ct);
            var normalized = (position ?? string.Empty).Trim().ToLowerInvariant();

            var isKitchen = LooksLikeKitchen(normalized);
            var isWaiter = LooksLikeWaiter(normalized);

            if (!isKitchen && !isWaiter)
            {
                // Fallback: if position isn't set, join both so demo stays functional.
                isKitchen = true;
                isWaiter = true;
            }

            if (isKitchen)
            {
                groups.Add(OrdersHubGroups.Kitchen(tenantId));
            }

            if (isWaiter)
            {
                groups.Add(OrdersHubGroups.Waiter(tenantId));
            }

            return groups;
        }

        private async Task<string?> GetPositionAsync(ClaimsPrincipal user, CancellationToken ct)
        {
            var subject =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                user.FindFirst("sub")?.Value ??
                user.FindFirst("subject")?.Value;

            if (Guid.TryParse(subject, out var userId))
            {
                var profile = await _db.EmployeeProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == userId, ct);
                return profile?.Position;
            }

            if (int.TryParse(subject, out var legacyId))
            {
                var profile = await _db.EmployeeProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.AppUserId == legacyId, ct);
                return profile?.Position;
            }

            return null;
        }

        private static bool LooksLikeKitchen(string position)
        {
            return position.Contains("kitchen") ||
                position.Contains("chef") ||
                position.Contains("cook") ||
                position.Contains("kuhar") ||
                position.Contains("kuhinja");
        }

        private static bool LooksLikeWaiter(string position)
        {
            return position.Contains("waiter") ||
                position.Contains("server") ||
                position.Contains("konobar");
        }
    }
}
