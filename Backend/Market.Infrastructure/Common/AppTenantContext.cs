using System.Security.Claims;
using Market.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Market.Infrastructure.Common;

public sealed class AppTenantContext(IHttpContextAccessor accessor) : ITenantContext
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public Guid? TenantId
    {
        get
        {
            var raw = User?.FindFirstValue("tenant_id");
            if (!Guid.TryParse(raw, out var parsed))
            {
                return null;
            }

            return parsed == Guid.Empty ? null : parsed;
        }
    }

    public Guid? RestaurantId
    {
        get
        {
            var raw = User?.FindFirstValue("restaurant_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : null;
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            var user = User;
            if (user?.Identity?.IsAuthenticated != true) return false;

            foreach (var claim in user.Claims)
            {
                if ((claim.Type == "role" || claim.Type == ClaimTypes.Role) &&
                    string.Equals(claim.Value, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
