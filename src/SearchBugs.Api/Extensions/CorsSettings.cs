namespace SearchBugs.Api.Extensions;

public class CorsSettings
{
    public const string SectionName = "CorsSettings";

    public List<string> AllowedOrigins { get; set; } = new();
    public bool AllowCredentials { get; set; } = true;
    public bool AllowAnyHeader { get; set; } = true;
    public bool AllowAnyMethod { get; set; } = true;
    public bool AllowAnyOrigin { get; set; } = false;
    public List<string> AllowedHeaders { get; set; } = new();
    public List<string> AllowedMethods { get; set; } = new();
    public List<string> ExposedHeaders { get; set; } = new();
    public TimeSpan PreflightMaxAge { get; set; } = TimeSpan.FromMinutes(30);
}
