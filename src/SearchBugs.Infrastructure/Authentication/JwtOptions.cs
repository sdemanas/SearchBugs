using System.ComponentModel.DataAnnotations;

namespace SearchBugs.Infrastructure.Authentication;

public class JwtOptions
{
    [Required]
    public required string Issuer { get; set; }
    [Required]
    public required string Audience { get; set; }
    [Required]
    [MinLength(16)]
    public required string Secret { get; set; }
    [Required]
    [Range(1, int.MaxValue)]
    public int ExpiryMinutes { get; set; }
}
