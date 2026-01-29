using System.Security.Claims;
using Market.API.Dtos.Notifications;
using Market.Application.Abstractions;
using Market.Domain.Entities.Notifications;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Market.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = PolicyNames.StaffMember)]
    public sealed class NotificationsController : ControllerBase
    {
        private readonly IAppDbContext _db;

        public NotificationsController(IAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<NotificationListResponse> List(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool unreadOnly = false,
            [FromQuery] string? type = null,
            CancellationToken ct = default)
        {
            var userId = ResolveUserId(User);
            var roles = await ResolveRolesAsync(User, ct);
            var tenantId = ResolveTenantId(User);

            var baseQuery = _db.Notifications
                .AsNoTracking()
                .Where(n => MatchesTarget(n, userId, roles));

            if (tenantId.HasValue)
            {
                // TODO: enforce tenant scoping when tenant context is mandatory.
                baseQuery = baseQuery.Where(n => n.TenantId == tenantId.Value);
            }

            var unreadCount = await baseQuery
                .Where(n => n.ReadAtUtc == null)
                .CountAsync(ct);

            if (unreadOnly)
            {
                baseQuery = baseQuery.Where(n => n.ReadAtUtc == null);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                baseQuery = baseQuery.Where(n => n.Type == type);
            }

            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 20 : pageSize > 100 ? 100 : pageSize;
            var skip = (pageNumber - 1) * pageSize;

            var totalCount = await baseQuery.CountAsync(ct);

            var items = await baseQuery
                .OrderByDescending(n => n.CreatedAtUtc)
                .Skip(skip)
                .Take(pageSize)
                .Select(n => new NotificationItemDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Link = n.Link,
                    CreatedAtUtc = n.CreatedAtUtc,
                    ReadAtUtc = n.ReadAtUtc
                })
                .ToListAsync(ct);

            return new NotificationListResponse
            {
                Items = items,
                TotalCount = totalCount,
                UnreadCount = unreadCount
            };
        }

        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
        {
            var userId = ResolveUserId(User);
            var roles = await ResolveRolesAsync(User, ct);
            var tenantId = ResolveTenantId(User);

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n =>
                    n.Id == id &&
                    MatchesTarget(n, userId, roles) &&
                    (!tenantId.HasValue || n.TenantId == tenantId.Value),
                    ct);

            if (notification == null)
            {
                return NotFound();
            }

            if (!notification.ReadAtUtc.HasValue)
            {
                notification.ReadAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }

            return NoContent();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        {
            var userId = ResolveUserId(User);
            var roles = await ResolveRolesAsync(User, ct);
            var tenantId = ResolveTenantId(User);

            var query = _db.Notifications
                .Where(n =>
                    n.ReadAtUtc == null &&
                    MatchesTarget(n, userId, roles) &&
                    (!tenantId.HasValue || n.TenantId == tenantId.Value));

            var items = await query.ToListAsync(ct);
            if (items.Count == 0)
            {
                return NoContent();
            }

            var now = DateTime.UtcNow;
            foreach (var notification in items)
            {
                notification.ReadAtUtc = now;
            }

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        private static bool MatchesTarget(NotificationEntity notification, string? userId, HashSet<string> roles)
        {
            var userMatch = !string.IsNullOrWhiteSpace(userId) &&
                string.Equals(notification.TargetUserId, userId, StringComparison.OrdinalIgnoreCase);

            var roleMatch = !string.IsNullOrWhiteSpace(notification.TargetRole) &&
                roles.Contains(notification.TargetRole);

            return userMatch || roleMatch;
        }

        private static string? ResolveUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user.FindFirstValue("sub") ??
                user.FindFirstValue("subject");
        }

        private async Task<HashSet<string>> ResolveRolesAsync(ClaimsPrincipal user, CancellationToken ct)
        {
            var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var claim in user.Claims)
            {
                if (claim.Type == "role" || claim.Type == ClaimTypes.Role)
                {
                    if (!string.IsNullOrWhiteSpace(claim.Value))
                    {
                        roles.Add(claim.Value);
                    }
                }
            }

            var position = await GetPositionAsync(user, ct);
            var normalized = (position ?? string.Empty).Trim().ToLowerInvariant();

            if (LooksLikeKitchen(normalized))
            {
                roles.Add("Kitchen");
            }

            if (LooksLikeWaiter(normalized))
            {
                roles.Add("Waiter");
            }

            if (!LooksLikeKitchen(normalized) && !LooksLikeWaiter(normalized))
            {
                // Fallback for demo data when position isn't set.
                roles.Add("Kitchen");
                roles.Add("Waiter");
            }

            return roles;
        }

        private async Task<string?> GetPositionAsync(ClaimsPrincipal user, CancellationToken ct)
        {
            var subject = ResolveUserId(user);
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

        private static Guid? ResolveTenantId(ClaimsPrincipal user)
        {
            var tenantIdClaim = user.FindFirstValue("tenant_id");
            if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return null;
            }

            if (tenantId == Guid.Empty || tenantId == SeedConstants.DefaultTenantId)
            {
                return null;
            }

            return tenantId;
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
