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
			// TODO: Nie odrzucaj wyjątku, tylko zaloguj go i zwróć OneOf aby zewnętrzny kod wiedział, że coś poszło nie tak i nie należy kontynuować
			logger.LogCritical(ex, "{ConfigurationName} configuration failed, see attached exception.", configurationName);
			throw;
		}

		logger.LogInformation("{ConfigurationName} configured.", configurationName);
	}
}
