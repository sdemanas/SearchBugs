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

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserId UserId
    {
        get
        {
            // Try different claim types for user ID
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier) ??
                             _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }
            return new UserId(userId);
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ??
                           _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

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
}