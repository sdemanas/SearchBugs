﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace SearchBugs.Infrastructure.Authentication;

public class JwtOptionsSetup : IConfigureOptions<JwtOptions>
{
    private const string JwtOptionsSection = "JwtOptions";
    private readonly IConfiguration _configuration;

    public JwtOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public void Configure(JwtOptions options)
    {
        _configuration.GetSection(JwtOptionsSection).Bind(options);
    }
}
