using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SearchBugs.Domain.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SearchBugs.Infrastructure.Authentication;

internal sealed class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _jwtOptions;

    // Custom claim types for impersonation
    private const string ImpersonatedUserIdClaimType = "impersonated_user_id";
    private const string OriginalUserIdClaimType = "original_user_id";
    private const string ImpersonatedEmailClaimType = "impersonated_email";

    public JwtProvider(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string GenerateJwtToken(User user)
    {
        // Debug: Log JWT values during token generation
        Console.WriteLine($"JwtProvider - Generating token with Issuer: {_jwtOptions.Issuer}, Audience: {_jwtOptions.Audience}, Secret Length: {_jwtOptions.Secret.Length}");

        var signCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret)),
            SecurityAlgorithms.HmacSha256
        );
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
        };

        // Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim("role", role.Name));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            signingCredentials: signCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateImpersonationJwtToken(User originalUser, User impersonatedUser)
    {
        var signCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret)),
            SecurityAlgorithms.HmacSha256
        );

        var claims = new List<Claim>
        {
            // Original user claims (the actual logged-in user)
            new Claim(JwtRegisteredClaimNames.Sub, originalUser.Id.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, originalUser.Email.Value),
            new Claim(OriginalUserIdClaimType, originalUser.Id.Value.ToString()),
            
            // Impersonated user claims
            new Claim(ImpersonatedUserIdClaimType, impersonatedUser.Id.Value.ToString()),
            new Claim(ImpersonatedEmailClaimType, impersonatedUser.Email.Value),
        };

        // Add role claims for the impersonated user (these are the active roles during impersonation)
        foreach (var role in impersonatedUser.Roles)
        {
            claims.Add(new Claim("role", role.Name));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            signingCredentials: signCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
