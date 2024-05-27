using System.ComponentModel.DataAnnotations;

namespace OnTrack.Backend.Api.Configuration;

public sealed class CorsOptions : IOptionsSection
{
	public static string SectionKey => "Cors";

	[Required]
	public bool Enabled { get; set; }

	public string[]? AllowedOrigins { get; set; }
	public string[]? AllowedMethods { get; set; }
	public string[]? AllowedHeaders { get; set; }

	public bool? AllowCredentials { get; set; }
}
