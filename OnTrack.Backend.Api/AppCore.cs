#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0058 // Expression value is never used
#pragma warning disable IDE0061 // Use expression body for local function

namespace OnTrack.Backend.Api;

internal static class AppCore
{
	public static void ConfigurationWrapper(Action configurationAction, string configurationName, ILogger<Program> logger)
	{
		using IDisposable? _ = logger.BeginScope(configurationName);

		logger.LogInformation("Configuring {ConfigurationName}...", configurationName);

		try
		{
			configurationAction();
		}
		catch (Exception ex)
		{
			logger.LogCritical(ex, "{ConfigurationName} configuration failed, see attached exception.", configurationName);
			throw;
		}

		logger.LogInformation("{ConfigurationName} configured.", configurationName);
	}
}
