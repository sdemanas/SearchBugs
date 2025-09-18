using SearchBugs.Domain.Users;

namespace SearchBugs.Application.Common.Interfaces;

/// <summary>
/// Service for accessing the current user's information
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the ID of the currently authenticated user (returns impersonated user ID if impersonating)
    /// </summary>
    UserId UserId { get; }

    /// <summary>
    /// Gets the email of the currently authenticated user (returns impersonated user email if impersonating)
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the username of the currently authenticated user
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Gets whether the current user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets whether the current user has the specified role
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if the user has the role, false otherwise</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Gets whether the current user has the specified permission
    /// </summary>
    /// <param name="permission">The permission to check</param>
    /// <returns>True if the user has the permission, false otherwise</returns>
    bool HasPermission(string permission);

    /// <summary>
    /// Gets the original user ID (the one actually logged in, not impersonated)
    /// Returns null if not impersonating
    /// </summary>
    UserId? OriginalUserId { get; }

    /// <summary>
    /// Checks if the current session is impersonating another user
    /// </summary>
    bool IsImpersonating { get; }

    /// <summary>
    /// Gets the actual logged-in user ID (returns original user ID if impersonating, otherwise current user ID)
    /// </summary>
    UserId ActualUserId { get; }
}
