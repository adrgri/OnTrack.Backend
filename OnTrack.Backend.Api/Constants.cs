using OnTrack.Backend.Api.Configuration;

namespace OnTrack.Backend.Api;

public static class Constants
{
	public static class ConfigurationKeys
	{
		public static string ConnectionStringsSectionPrefix => "ConnectionStrings:";
		public static string SqlDefault => "SqlDatabase";

		public static string Cors => CorsConfiguration.SectionKey;
		public static string Smtp => SmtpEmailServicesOptions.SectionKey;
	}
}
