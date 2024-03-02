namespace OnTrack.Backend.Api.Configuration;

public sealed class CorsConfiguration : IOptionsSection
{
	public static string SectionKey => "Cors";

	public string[]? AllowedOrigins { get; set; }
	public string[]? AllowedMethods { get; set; }
	public string[]? AllowedHeaders { get; set; }

	public bool? AllowCredentials { get; set; }
}
