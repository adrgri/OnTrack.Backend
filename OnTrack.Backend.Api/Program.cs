using OnTrack.Backend.Api;

using Serilog;
using Serilog.Debugging;

try
{
	SelfLog.Enable(Console.WriteLine);

	WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

	builder.AddConfigurationSources();

	ILogger<Program> logger = builder.ConfigureLogger();

	builder.ConfigureOptions(logger);
	builder.ConfigureServices(logger);
	builder.ConfigureDependencies(logger);
	builder.ConfigureWebHost(logger);

	WebApplication app = builder.BuildApplication(logger);

	app.ConfigureRequestPipeline(logger);

	app.SanityCheck(logger);

	app.Run(logger);
}
finally
{
	Log.Information("Disposing the logger.");
	Log.CloseAndFlush();
}
