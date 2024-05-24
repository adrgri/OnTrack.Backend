using OnTrack.Backend.Api;
using OnTrack.Backend.Api.DataAccess;

using Serilog;
using Serilog.Debugging;

try
{
#if DEBUG
	SelfLog.Enable(message =>
	{
		Console.WriteLine(message);
		System.Diagnostics.Debug.WriteLine(message);
	});
#endif

	WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

	builder.AddConfigurationSources();

	ILogger<Program> logger = builder.ConfigureLogger<Program>();

	builder.ConfigureOptions(logger);
	builder.ConfigureServices(logger);
	builder.ConfigureDependencies(logger);
	builder.ConfigureWebHost(logger);

	WebApplication app = builder.BuildApplication(logger);

	app.EnsureDatabaseCreated<AppDbContext>(logger);
	app.ConfigureRequestPipeline(logger);

	await app.RunAsync(logger);
}
finally
{
	Log.Information("Disposing the logger.");
	Log.CloseAndFlush();
}
