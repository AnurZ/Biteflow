using System;
using System.Collections.Generic;
using System.Security.Claims;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Market.API.Hubs
{
    public static class OrdersHubEvents
    {
        public const string OrderCreated = "OrderCreated";
        public const string OrderStatusChanged = "OrderStatusChanged";
        public const string NotificationCreated = "NotificationCreated";
        public const string NotificationCleared = "NotificationCleared";
    }

    public static class OrdersHubGroups
    {
        public static string User(string userId, Guid? tenantId)
        {
            var trimmed = (userId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            if (!IsTenantScoped(tenantId))
            {
                return $"user:{trimmed}";
            }

            return $"tenant:{tenantId}:user:{trimmed}";
        }

        public static string Role(string role, Guid? tenantId)
        {
            var trimmed = (role ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            if (!IsTenantScoped(tenantId))
            {
                return $"role:{trimmed}";
            }

            return $"tenant:{tenantId}:role:{trimmed}";
        }

        public static string Kitchen(Guid? tenantId)
        {
            if (!IsTenantScoped(tenantId))
            {
                return "kitchen";
            }

            return $"tenant:{tenantId}:kitchen";
        }

        public static string Waiter(Guid? tenantId)
        {
            if (!IsTenantScoped(tenantId))
            {
                return "waiter";
            }

            return $"tenant:{tenantId}:waiter";
        }

        private static bool IsTenantScoped(Guid? tenantId)
        {
            return tenantId.HasValue && tenantId.Value != Guid.Empty;
        }
    }

    [Authorize(Policy = PolicyNames.StaffMember)]
    public sealed class OrdersHub : Hub
    {
        private readonly ILogger<OrdersHub> _logger;

        public OrdersHub(ILogger<OrdersHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var tenantId = ResolveTenantId(Context.User);
            var groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (Context.User != null)
            {
                var userId = ResolveUserId(Context.User);
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    var userGroup = OrdersHubGroups.User(userId, tenantId);
                    if (!string.IsNullOrWhiteSpace(userGroup))
                    {
                        groups.Add(userGroup);
                    }
                }

                foreach (var role in ResolveRoleClaims(Context.User))
                {
                    if (string.Equals(role, RoleNames.Kitchen, StringComparison.OrdinalIgnoreCase))
                    {
                        groups.Add(OrdersHubGroups.Kitchen(tenantId));
                    }

                    if (string.Equals(role, RoleNames.Waiter, StringComparison.OrdinalIgnoreCase))
                    {
                        groups.Add(OrdersHubGroups.Waiter(tenantId));
                    }

                    var roleGroup = OrdersHubGroups.Role(role, tenantId);
                    if (!string.IsNullOrWhiteSpace(roleGroup))
                    {
                        groups.Add(roleGroup);
                    }
                }
            }

            foreach (var group in groups)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
            }

            _logger.LogInformation("OrdersHub connected {ConnectionId}. Groups={Groups}", Context.ConnectionId, string.Join(",", groups));
            await base.OnConnectedAsync();
        }

        private static string? ResolveUserId(ClaimsPrincipal? user)
        {
            if (user == null)
            {
                return null;
            }

            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                user.FindFirst("sub")?.Value ??
                user.FindFirst("subject")?.Value;
        }

        private static Guid? ResolveTenantId(ClaimsPrincipal? user)
        {
            var tenantId = user?.FindFirst("tenant_id")?.Value;
            if (!Guid.TryParse(tenantId, out var parsed) || parsed == Guid.Empty)
            {
                return null;
            }

            return parsed;
        }

        private static IEnumerable<string> ResolveRoleClaims(ClaimsPrincipal? user)
        {
            if (user == null)
            {
                yield break;
            }

            foreach (var claim in user.Claims)
            {
                if (claim.Type == "role" || claim.Type == ClaimTypes.Role)
                {
                    yield return claim.Value;
                }
            }
        }
    }
}
