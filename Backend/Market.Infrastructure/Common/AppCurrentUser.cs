using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Market.Application.Abstractions;

namespace Market.Infrastructure.Common;

/// <summary>
/// Implementation of IAppCurrentUser that reads data from a JWT token.
/// </summary>
public sealed class AppCurrentUser(IHttpContextAccessor httpContextAccessor)
    : IAppCurrentUser
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        TryParseGuidClaim(ClaimTypes.NameIdentifier)
        ?? TryParseGuidClaim("sub");

    public string? Email =>
        _user?.FindFirstValue(ClaimTypes.Email)
        ?? _user?.FindFirstValue("email");

    public bool IsAuthenticated =>
        _user?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyCollection<string> Roles =>
        _user?.Claims
            .Where(claim => claim.Type == ClaimTypes.Role || claim.Type == "role")
            .Select(claim => claim.Value)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? Array.Empty<string>();

    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    private Guid? TryParseGuidClaim(string claimType)
    {
        var raw = _user?.FindFirstValue(claimType);
        return Guid.TryParse(raw, out var parsed) ? parsed : null;
    }
}
