using OnTrack.Backend.Api.Configuration;

namespace OnTrack.Backend.Api;

public static class Constants
{
	public static class ConfigurationSectionKeys
	{
		public static string ConnectionStrings => ConnectionsConfiguration.SectionKey;
		public static string Cors => CorsConfiguration.SectionKey;
		public static string Smtp => SmtpServicesOptions.SectionKey;
	}
}
