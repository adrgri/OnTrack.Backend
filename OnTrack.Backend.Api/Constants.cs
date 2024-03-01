using OnTrack.Backend.Api.Configuration;

namespace OnTrack.Backend.Api;

public static class Constants
{
	public static class ConfigurationKeys
	{
		public const string SqlDefaultDatabase = "SqlDatabase";
	}

	public static class ConfigurationSectionKeys
	{
		public const string ConnectionStrings = "ConnectionStrings";

		public static string Cors => CorsConfiguration.SectionKey;
		public static string Smtp => SmtpEmailServicesOptions.SectionKey;
	}
}
