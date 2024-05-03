#if DEBUG

using System.Diagnostics;
using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/debug")]
public sealed class DebugController(ILogger<DebugController> logger, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
	: ControllerBase
{
	private readonly ILogger<DebugController> _logger = logger;
	private readonly IConfiguration _configuration = configuration;
	private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

	private static T? GetValue<T>(IConfiguration configuration, string key, ILogger logger)
	{
		T? value = configuration.GetValue<T>(key);

		if (value is null)
		{
			logger.LogError("The configuration key {ConfigKey} is not present in the current IConfiguration manager.", key);
		}

		return value;
	}

	private PhysicalFileResult SendPhysicalFile(FileInfo file)
	{
		const string fileMime = "application/octet-stream";

		return PhysicalFile(file.FullName, fileMime, file.Name);
	}

	[Authorize]
	[HttpGet("testAuthentication")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public IActionResult TestAuthentication()
	{
		return Ok("You have authenticated successfully :)");
	}

	[HttpGet("environmentInfo")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public IActionResult EnvironmentInfo()
	{
		return Ok(new
		{
			_webHostEnvironment.EnvironmentName,
			isNotProduction = _webHostEnvironment.IsProduction() == false
		});
	}

	[HttpGet("downloadSqliteLogDatabase")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public IActionResult DownloadSqliteLogDatabase()
	{
		using IDisposable? _ = _logger.BeginScope(nameof(DownloadSqliteLogDatabase));

		// TODO Przenieś ten klucz konfiguracyjny do klasy ConfigurationSectionKeys i ustaw dla niego walidację
		string? sqliteDbPath = GetValue<string>(_configuration, "Serilog:WriteTo:0:Args:sqliteDbPath", _logger);

		if (sqliteDbPath is null)
		{
			return NotFound();
		}

		FileInfo sqliteDbFileInfo = new(sqliteDbPath);

		if (sqliteDbFileInfo.Exists)
		{
			return SendPhysicalFile(sqliteDbFileInfo);
		}

		// At this point we know that the path in the config is not absolute, so we need to construct it manually based (probably) on the current assembly directory.
		string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new UnreachableException();

		FileInfo sqliteDbAbsoluteFileInfo = new(Path.Combine(assemblyDirectory, sqliteDbPath));

		if (sqliteDbAbsoluteFileInfo.Exists)
		{
			return SendPhysicalFile(sqliteDbAbsoluteFileInfo);
		}

		_logger.LogError(
			"Tried to send the log database file using the following paths [ {RelativePath}, {AbsolutePath} ], but the file does not exist under either of them.",
			sqliteDbFileInfo.FullName,
			sqliteDbAbsoluteFileInfo.FullName);

		return NotFound();
	}
}
#endif
