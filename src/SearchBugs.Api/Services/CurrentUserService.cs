using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Users;
using Shared.Primitives;
using System.IdentityModel.Tokens.Jwt;

namespace SearchBugs.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Custom claim types for impersonation
    private const string ImpersonatedUserIdClaimType = "impersonated_user_id";
    private const string OriginalUserIdClaimType = "original_user_id";
    private const string ImpersonatedEmailClaimType = "impersonated_email";

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserId UserId
    {
        get
        {
            // Check for impersonation first - if impersonating, return the impersonated user ID
            var impersonatedUserIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ImpersonatedUserIdClaimType);
            if (impersonatedUserIdClaim != null && Guid.TryParse(impersonatedUserIdClaim.Value, out var impersonatedUserId))
            {
                return new UserId(impersonatedUserId);
            }

            // Fall back to regular user ID claims
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier) ??
                             _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }
            return new UserId(userId);
        }
    }

    public string? Email => GetCurrentEmail();

    public string? Username => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
    }

    public bool HasPermission(string permission)
    {
        return _httpContextAccessor.HttpContext?.User.HasClaim("Permission", permission) ?? false;
    }

    /// <summary>
    /// Gets the original user ID (the one actually logged in, not impersonated)
    /// </summary>
    public UserId? OriginalUserId
    {
        get
        {
            var originalUserIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(OriginalUserIdClaimType);
            if (originalUserIdClaim != null && Guid.TryParse(originalUserIdClaim.Value, out var originalUserId))
            {
                return new UserId(originalUserId);
            }
            return null; // No impersonation happening
        }
    }

    /// <summary>
    /// Checks if the current session is impersonating another user
    /// </summary>
    public bool IsImpersonating => _httpContextAccessor.HttpContext?.User.FindFirst(ImpersonatedUserIdClaimType) != null;

    /// <summary>
    /// Gets the actual logged-in user ID (returns original user ID if impersonating, otherwise current user ID)
    /// </summary>
    public UserId ActualUserId => OriginalUserId ?? UserId;

    private string? GetCurrentEmail()
    {
        // Check for impersonated email first
        var impersonatedEmail = _httpContextAccessor.HttpContext?.User.FindFirst(ImpersonatedEmailClaimType)?.Value;
        if (!string.IsNullOrEmpty(impersonatedEmail))
        {
            return impersonatedEmail;
        }

        // Fall back to regular email claims
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ??
               _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
    }
}