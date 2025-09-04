namespace SearchBugs.Infrastructure.Options;

public class PermissionCacheOptions
{
    public const string ConfigurationSection = "PermissionCache";

    /// <summary>
    /// Cache expiry duration in minutes. Default is 30 minutes.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 30;

    /// <summary>
    /// Sliding expiration in minutes. Default is 10 minutes.
    /// </summary>
    public int SlidingExpiryMinutes { get; set; } = 10;

    /// <summary>
    /// Whether caching is enabled. Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Cache provider type. Options: Memory, Redis
    /// </summary>
    public string Provider { get; set; } = "Memory";

    /// <summary>
    /// Redis connection string (only used when Provider is Redis)
    /// </summary>
    public string? RedisConnectionString { get; set; }
}
