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
            var roles = ResolveRoles(User);
            var tenantId = ResolveTenantId(User);

            var baseQuery = ApplyTargetFilter(
                _db.Notifications.AsNoTracking(),
                userId,
                roles);

            if (tenantId.HasValue)
            {
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
            var roles = ResolveRoles(User);
            var tenantId = ResolveTenantId(User);

            var query = ApplyTargetFilter(_db.Notifications, userId, roles)
                .Where(n => n.Id == id);

            if (tenantId.HasValue)
            {
                query = query.Where(n => n.TenantId == tenantId.Value);
            }

            var notification = await query.FirstOrDefaultAsync(ct);

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
            var roles = ResolveRoles(User);
            var tenantId = ResolveTenantId(User);

            var query = ApplyTargetFilter(_db.Notifications, userId, roles)
                .Where(n => n.ReadAtUtc == null);

            if (tenantId.HasValue)
            {
                query = query.Where(n => n.TenantId == tenantId.Value);
            }

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

        private static IQueryable<NotificationEntity> ApplyTargetFilter(
            IQueryable<NotificationEntity> query,
            string? userId,
            HashSet<string> roles)
        {
            var normalizedUserId = string.IsNullOrWhiteSpace(userId)
                ? null
                : userId.Trim().ToLower();
            var normalizedRoles = roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim().ToLower())
                .Distinct()
                .ToArray();

            if (normalizedUserId is null && normalizedRoles.Length == 0)
            {
                return query.Where(_ => false);
            }

            return query.Where(n =>
                (normalizedUserId != null &&
                 n.TargetUserId != null &&
                 n.TargetUserId.ToLower() == normalizedUserId) ||
                (normalizedRoles.Length > 0 &&
                 n.TargetRole != null &&
                 normalizedRoles.Contains(n.TargetRole.ToLower())));
        }

        private static string? ResolveUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user.FindFirstValue("sub") ??
                user.FindFirstValue("subject");
        }

        private static HashSet<string> ResolveRoles(ClaimsPrincipal user)
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

            return roles;
        }

        private static Guid? ResolveTenantId(ClaimsPrincipal user)
        {
            var tenantIdClaim = user.FindFirstValue("tenant_id");
            if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return null;
            }

            if (tenantId == Guid.Empty)
            {
                return null;
            }

            return tenantId;
        }
    }
}
