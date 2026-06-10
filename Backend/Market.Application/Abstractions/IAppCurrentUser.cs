namespace Market.Application.Abstractions;

/// <summary>
/// Represents the currently logged-in user in the system.
/// </summary>
public interface IAppCurrentUser
{
    /// <summary>
    /// User identifier (UserId).
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// User Email. (optional)
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Indicates whether the user is logged in.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Roles assigned to the current user.
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>
    /// Indicates whether the current user has the specified role.
    /// </summary>
    bool IsInRole(string role);
}
