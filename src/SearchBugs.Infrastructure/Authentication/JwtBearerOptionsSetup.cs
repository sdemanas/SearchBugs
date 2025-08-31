using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SearchBugs.Infrastructure.Authentication;

public class JwtBearerOptionsSetup : IConfigureOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions;

    public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public void Configure(JwtBearerOptions options)
    {
        // Add validation to ensure the secret is not null or empty
        if (string.IsNullOrEmpty(_jwtOptions.Secret))
        {
            throw new InvalidOperationException("JWT Secret is not configured properly");
        }

        Console.WriteLine($"JwtBearerOptionsSetup.Configure called!");
        Console.WriteLine($"JWT Configuration - Issuer: {_jwtOptions.Issuer}, Audience: {_jwtOptions.Audience}, Secret Length: {_jwtOptions.Secret.Length}");

        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        Console.WriteLine($"SymmetricKey created with key size: {symmetricKey.KeySize}");

        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = symmetricKey,
            // Don't map inbound claims - keep them as they are in the JWT
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role",
            ClockSkew = TimeSpan.Zero // Remove clock skew tolerance
        };

        // Ensure claims are not mapped to different types
        options.MapInboundClaims = false;

        Console.WriteLine($"JwtBearerOptions configured successfully");
    }
}
