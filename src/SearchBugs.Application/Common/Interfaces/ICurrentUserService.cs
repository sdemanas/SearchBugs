using SearchBugs.Domain.Users;
using Shared.Primitives;

namespace SearchBugs.Application.Common.Interfaces;

/// <summary>
/// Service for accessing the current user's information
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the ID of the currently authenticated user
    /// </summary>
    UserId UserId { get; }

    /// <summary>
    /// Gets the email of the currently authenticated user
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
} 