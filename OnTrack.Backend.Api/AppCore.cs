namespace OnTrack.Backend.Api;

internal static class AppCore
{
	public static void ConfigurationWrapper(Action configurationAction, string configurationName, ILogger logger)
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
