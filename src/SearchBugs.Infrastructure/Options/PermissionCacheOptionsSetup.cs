using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace SearchBugs.Infrastructure.Options;

public class PermissionCacheOptionsSetup : IConfigureOptions<PermissionCacheOptions>
{
    private const string ConfigurationSectionName = PermissionCacheOptions.ConfigurationSection;
    private readonly IConfiguration _configuration;

    public PermissionCacheOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(PermissionCacheOptions options)
    {
        _configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}
