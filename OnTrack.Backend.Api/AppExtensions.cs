#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0058 // Expression value is never used
#pragma warning disable IDE0061 // Use expression body for local function

using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using OneOf;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Configuration;
using OnTrack.Backend.Api.DataAccess;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;
using OnTrack.Backend.Api.Validation;

using Serilog;

using Swashbuckle.AspNetCore.SwaggerGen;

using static OnTrack.Backend.Api.AppCore;

namespace OnTrack.Backend.Api;

internal static class AppExtensions
{
	public static void AddConfigurationSources(this WebApplicationBuilder builder)
	{
		builder.Configuration.AddEnvironmentVariables();
	}

	public static ILogger<Program> ConfigureLogger(this WebApplicationBuilder builder)
	{
		builder.Logging.ClearProviders();

		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(builder.Configuration)
			.CreateLogger();

		builder.Host.UseSerilog(Log.Logger, dispose: true);

		ILogger<Program> logger = new LoggerFactory()
			.AddSerilog(Log.Logger)
			.CreateLogger<Program>();

		logger.LogInformation("{ConfigurationName} configured.", "Logger");

		return logger;
	}

	private static void ConfigureSmtpEmailServicesOptions(this WebApplicationBuilder builder)
	{
		builder.Services.AddOptions<SmtpEmailServicesOptions>()
			.Bind(builder.Configuration.GetRequiredSection(ConfigurationKeys.Smtp))
			.ValidateDataAnnotations()
			.ValidateOnStart();
	}

	public static void ConfigureOptions(this WebApplicationBuilder builder, ILogger<Program> logger)
	{
		ConfigurationWrapper(() =>
		{
			logger.LogInformation("Adding {options}...", nameof(SmtpEmailServicesOptions));
			builder.ConfigureSmtpEmailServicesOptions();
		}, "Options", logger);
	}

	private static void ConfigureCors(this WebApplicationBuilder builder, ILogger<Program> logger)
	{
		logger.LogInformation("Configuring {ConfigurationName}...", "Default CORS policy");

		string corsConfigurationSectionKey = ConfigurationKeys.Cors;

		CorsConfiguration? corsConfiguration = builder.Configuration
			.GetSection(corsConfigurationSectionKey)
			.Get<CorsConfiguration>();

		if (corsConfiguration is null)
		{
			logger.LogInformation("CORS configuration section named {CorsConfigurationKey} was not found.", corsConfigurationSectionKey);
			logger.LogInformation("CORS is disabled.");
			return;
		}

		logger.LogWarning("CORS configuration section found.");

		builder.Services.AddCors(options => options.AddDefaultPolicy(corsPolicyBuilder =>
		{
			logger.LogWarning("Enabling {ConfigurationName}...", "Default CORS policy");

			if (corsConfiguration.AllowedOrigins is not null)
			{
				corsPolicyBuilder.WithOrigins(corsConfiguration.AllowedOrigins);

				logger.LogWarning("CORS is enabled for the following origins: {AllowedOrigins}.", (object)corsConfiguration.AllowedOrigins);
			}

			if (corsConfiguration.AllowedMethods is not null)
			{
				corsPolicyBuilder.WithMethods(corsConfiguration.AllowedMethods);

				logger.LogWarning("CORS is enabled for the following methods: {AllowedMethods}.", (object)corsConfiguration.AllowedMethods);
			}

			if (corsConfiguration.AllowedHeaders is not null)
			{
				corsPolicyBuilder.WithHeaders(corsConfiguration.AllowedHeaders);

				logger.LogWarning("CORS is enabled for the following headers: {AllowedHeaders}.", (object)corsConfiguration.AllowedHeaders);
			}

			if (corsConfiguration.AllowCredentials is not null)
			{
				const string message = "CORS credentials are {AllowCredentials}.";

				if (corsConfiguration.AllowCredentials.Value)
				{
					corsPolicyBuilder.AllowCredentials();

					logger.LogWarning(message, "Allowed");
				}
				else
				{
					corsPolicyBuilder.DisallowCredentials();

					logger.LogInformation(message, "Disallowed");
				}
			}

			logger.LogWarning("{ConfigurationName} configured.", "Default CORS policy");
		}));
	}

	private static IEnumerable<Type> GetStronglyTypedIds()
	{
		return Assembly.GetExecutingAssembly().GetTypes()
			.Where(mytype => mytype.GetInterfaces().Contains(typeof(IStronglyTypedId)));
	}

	private static void MapAllStronglyTypedIds(SwaggerGenOptions options)
	{
		static OpenApiSchema CommonSchemaSetup() => new()
		{
			Type = "string",
			Format = "uuid"
		};

		foreach (Type stronglyTypedId in GetStronglyTypedIds())
		{
			options.MapType(stronglyTypedId, CommonSchemaSetup);
		}
	}

	private static IEnumerable<TAttribute> SearchAssemblyForJsonConverterAttributes<TAttribute>()
		where TAttribute : Attribute
	{
		return typeof(AppExtensions).Assembly.GetTypes()
			.Select(type => type.GetCustomAttribute<TAttribute>())
			.Where(attribute => attribute is not null)!;
		// Avoided unnecessary Cast<TAttribute>() by using null-forgiving operator since the collection will not contain any null elements
	}

	private static IEnumerable<Type> SearchAssemblyForJsonConverterAttributesAndExtractConverterTypes(ILogger<Program> logger)
	{
		Type Converter(JsonConverterAttribute jsonConverterAttribute)
		{
			Type? converterType = jsonConverterAttribute.ConverterType;

			if (converterType is null)
			{
				logger.LogError(
					new UnreachableException($"This should not happen since {nameof(JsonConverterAttribute)} enforces this property in the constructor."),
					"{Property} property of the {Object} was null.",
					nameof(JsonConverterAttribute.ConverterType),
					nameof(JsonConverterAttribute));

				throw new InvalidOperationException($"{nameof(JsonConverterAttribute.ConverterType)} is not set properly.");
			}

			return converterType;
		}

		return SearchAssemblyForJsonConverterAttributes<JsonConverterAttribute>().Select(Converter);
	}

	private static JsonConverter ActivateJsonConverter(Type jsonConverterType, ILogger<Program> logger)
	{
		try
		{
			object activated = Activator.CreateInstance(jsonConverterType)
				?? throw new UnreachableException($"Object ({nameof(activated)}) returned by the {nameof(Activator)}.{nameof(Activator.CreateInstance)} is null.");

			return activated switch
			{
				JsonConverter converter => converter,
				_ => throw new UnreachableException($"Attempted to cast object of type {activated.GetType()} to {typeof(JsonConverter)}.")
			};
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Could not create an instance of {JsonConverterType}.", jsonConverterType);
			throw;
		}
	}

	private static void AddStronglyTypedIdJsonConverters(JsonOptions options, ILogger<Program> logger)
	{
		IEnumerable<JsonConverter> activatedJsonConverters = SearchAssemblyForJsonConverterAttributesAndExtractConverterTypes(logger)
			.Select(jsonConverterType => ActivateJsonConverter(jsonConverterType, logger));

		foreach (JsonConverter jsonConverter in activatedJsonConverters)
		{
			logger.LogInformation("Registering JsonConverter of type {JsonConverterType}.", jsonConverter.GetType());

			options.JsonSerializerOptions.Converters.Add(jsonConverter);
		}
	}

	/// <summary>
	/// Adds services to the container
	/// </summary>
	public static void ConfigureServices(this WebApplicationBuilder builder, ILogger<Program> logger)
	{
		ConfigurationWrapper(() =>
		{
			builder.Services.AddHsts(options => options.IncludeSubDomains = true);
			builder.Services.AddAntiforgery();

			builder.Services.AddDbContext<AppDbContext>(options =>
			{
				string sqlDefaultConnectionString = string.Concat(ConfigurationKeys.ConnectionStringsSectionPrefix, ConfigurationKeys.SqlDefault);

				options.UseSqlServer(builder.Configuration.GetRequiredSection(sqlDefaultConnectionString).Value);

				if (builder.Environment.IsProduction() == false)
				{
					options.EnableDetailedErrors();
				}
			} /* If you ever see concurrent exception being thrown by EF Core this might help: contextLifetime: ServiceLifetime.Transient */);

			if (builder.Environment.IsProduction() == false)
			{
				builder.Services.AddDatabaseDeveloperPageExceptionFilter();
			}

			builder.ConfigureCors(logger);

			builder.Services.AddAuthentication();
			builder.Services.AddAuthorization();

			builder.Services.AddHealthChecks();

			builder.Services.AddIdentityApiEndpoints<AppUser>()
				.AddEntityFrameworkStores<AppDbContext>()
				.AddDefaultTokenProviders()
				.AddApiEndpoints();

			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options => MapAllStronglyTypedIds(options));

			builder.Services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
				AddStronglyTypedIdJsonConverters(options, logger);
			});

			builder.Services.AddApplicationInsightsTelemetry();
		}, "Services", logger);
	}

	private static void ConfigureEmailServices(WebApplicationBuilder builder, ILogger<Program> logger)
	{
		ConfigurationWrapper(() =>
		{
			SmtpEmailServicesOptions? smtpEmailServicesOptions = builder.Configuration
				.GetSection(ConfigurationKeys.Smtp)
				.Get<SmtpEmailServicesOptions>();

			if (smtpEmailServicesOptions?.Enabled == true)
			{
				builder.Services.AddTransient<IEmailSender<AppUser>, EmailSenderService<AppUser>>();
			}
		}, "Email services", logger);
	}

	private static void ConfigureDataAccessServices(WebApplicationBuilder builder, ILogger<Program> logger)
	{
		ConfigurationWrapper(() =>
		{
			builder.Services.AddTransient<IEntityAccessService<AppUser, IdentitySystemObjectId>, EfAppUsersAccessService<AppDbContext>>();
			builder.Services.AddTransient<IEntityAccessService<Attachment, AttachmentId>, EfEntityAccessService<Attachment, AttachmentId, AppDbContext>>();
			builder.Services.AddTransient<IEntityAccessService<Icon, IconId>, EfEntityAccessService<Icon, IconId, AppDbContext>>();
			builder.Services.AddTransient<IEntityAccessService<Language, LanguageId>, EfEntityAccessService<Language, LanguageId, AppDbContext>>();
			builder.Services.AddTransient<IEntityAccessService<Milestone, MilestoneId>, EfMilestonesAccessService<AppDbContext>>();
			builder.Services.AddTransient<IEntityAccessService<Project, ProjectId>, EfProjectsAccessService<AppDbContext>>();
			builder.Services.AddTransient<IEntityAccessService<Resource, ResourceId>, EfEntityAccessService<Resource, ResourceId, AppDbContext>>();
			builder.Services.AddTransient<IEntityAccessService<Status, StatusId>, EfEntityAccessService<Status, StatusId, AppDbContext>>();
			builder.Services.AddTransient<IEntityAccessService<Task, TaskId>, EfTasksAccessService<AppDbContext>>();
		}, "Data access services", logger);
	}

	private static void ConfigureModelMappings(WebApplicationBuilder builder, ILogger<Program> logger)
	{
		ConfigurationWrapper(() =>
		{
			builder.Services.AddSingleton<IMapper<AppUser, IdentitySystemObjectId, AppUserDto>, AppUserMapper>();
			builder.Services.AddSingleton<IMapper<Attachment, AttachmentId, AttachmentDto>, AttachmentMapper>();
			builder.Services.AddSingleton<IMapper<Icon, IconId, IconDto>, IconMapper>();
			builder.Services.AddSingleton<IMapper<Language, LanguageId, LanguageDto>, LanguageMapper>();
			builder.Services.AddSingleton<IMapper<Milestone, MilestoneId, MilestoneDto>, MilestoneMapper>();
			builder.Services.AddSingleton<IMapper<Project, ProjectId, ProjectDto>, ProjectMapper>();
			builder.Services.AddSingleton<IMapper<Resource, ResourceId, ResourceDto>, ResourceMapper>();
			builder.Services.AddSingleton<IMapper<Status, StatusId, StatusDto>, StatusMapper>();
			builder.Services.AddSingleton<IMapper<Task, TaskId, TaskDto>, TaskMapper>();
		}, "Model mappers", logger);
	}

	private static void ConfigureValidators(WebApplicationBuilder builder, ILogger<Program> logger)
	{
		ConfigurationWrapper(() =>
		{
			builder.Services.AddTransient<IAsyncCollectionValidator<IdentitySystemObjectId, OneOf<AppUser, EntityIdErrorsDescription<IdentitySystemObjectId>>>, EntityByIdExistenceValidator<IdentitySystemObjectId, AppUser>>();
			builder.Services.AddTransient<IAsyncCollectionValidator<AttachmentId, OneOf<Attachment, EntityIdErrorsDescription<AttachmentId>>>, EntityByIdExistenceValidator<AttachmentId, Attachment>>();
			builder.Services.AddTransient<IAsyncCollectionValidator<IconId, OneOf<Icon, EntityIdErrorsDescription<IconId>>>, EntityByIdExistenceValidator<IconId, Icon>>();
			builder.Services.AddTransient<IAsyncCollectionValidator<LanguageId, OneOf<Language, EntityIdErrorsDescription<LanguageId>>>, EntityByIdExistenceValidator<LanguageId, Language>>();
			builder.Services.AddTransient<IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>>, EntityByIdExistenceValidator<MilestoneId, Milestone>>();
			builder.Services.AddTransient<IAsyncCollectionValidator<ProjectId, OneOf<Project, EntityIdErrorsDescription<ProjectId>>>, EntityByIdExistenceValidator<ProjectId, Project>>();
			builder.Services.AddTransient<IAsyncCollectionValidator<ResourceId, OneOf<Resource, EntityIdErrorsDescription<ResourceId>>>, EntityByIdExistenceValidator<ResourceId, Resource>>();
			builder.Services.AddTransient<IAsyncCollectionValidator<StatusId, OneOf<Status, EntityIdErrorsDescription<StatusId>>>, EntityByIdExistenceValidator<StatusId, Status>>();
			builder.Services.AddTransient<IAsyncCollectionValidator<TaskId, OneOf<Task, EntityIdErrorsDescription<TaskId>>>, EntityByIdExistenceValidator<TaskId, Task>>();
		}, "Validators", logger);
	}

	public static void ConfigureDependencies(this WebApplicationBuilder builder, ILogger<Program> logger)
	{
		ConfigurationWrapper(() =>
		{
			ConfigureEmailServices(builder, logger);
			ConfigureDataAccessServices(builder, logger);
			ConfigureModelMappings(builder, logger);
			ConfigureValidators(builder, logger);
		}, "Dependencies", logger);
	}

	public static void ConfigureWebHost(this WebApplicationBuilder builder, ILogger<Program> logger)
	{
		ConfigurationWrapper(() => builder.WebHost.UseQuic(), nameof(builder.WebHost), logger);
	}

	public static WebApplication BuildApplication(this WebApplicationBuilder builder, ILogger<Program> logger)
	{
		WebApplication app;

		//ConfigurationCore(() =>
		//{
		//	app = builder.Build();
		//}, "Building", logger);

		logger.LogInformation("Building application...");

		try
		{
			app = builder.Build();
		}
		catch (Exception ex)
		{
			logger.LogCritical(ex, "Application failed to build, see attached exception.");
			throw;
		}

		logger.LogInformation("Application build successfully.");

		return app;
	}

	///	<summary>
	///	Configures the HTTP request pipeline
	/// </summary>
	/// <remarks>
	/// Order is important when configuring the pipeline!
	/// </remarks>
	public static void ConfigureRequestPipeline(this WebApplication app, ILogger<Program> logger)
	{
		ConfigurationWrapper(() =>
		{
			app.UseHttpsRedirection();
			app.UseHsts();
			app.UseAntiforgery();

			app.UseSerilogRequestLogging();

			if (app.Environment.IsProduction() == false)
			{
				app.UseDeveloperExceptionPage();
				app.UseMigrationsEndPoint();
			}

			app.UseSwagger();
			app.UseSwaggerUI();

			if (app.Environment.IsProduction() == false)
			{
				app.UseCors();
			}

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapGroup("/api/identity/").MapIdentityApi<AppUser>().WithOpenApi(options =>
			{
				options.Tags.Clear();
				options.Tags.Add(new OpenApiTag() { Name = "Identity" });

				return options;
			});

			app.MapControllers();
		}, "Pipeline", logger);
	}

	private static void LogSmtpConfiguration(WebApplication app, ILogger<Program> logger)
	{
		SmtpEmailServicesOptions smtpEmailServicesOptions = app.Services.GetRequiredService<IOptions<SmtpEmailServicesOptions>>().Value;

		logger.LogInformation("SMTP Email Services {SmtpEmailServicesEnabled}.", smtpEmailServicesOptions.Enabled ? "Enabled" : "Disabled");

		if (smtpEmailServicesOptions.Enabled)
		{
			logger.LogInformation("Current SMTP config is {Config}.", JsonSerializer.Serialize(smtpEmailServicesOptions));
		}
	}

	// TODO Zmień nazwę tej metody na jakąś inną, w zależności od tego, co ona będzie robiła w przyszłości
	public static void SanityCheck(this WebApplication app, ILogger<Program> logger)
	{
		logger.LogInformation("Performing sanity checks...");

		try
		{
			LogSmtpConfiguration(app, logger);
		}
		catch (Exception ex)
		{
			logger.LogCritical(ex, "Application will not run with current configuration, see attached exception.");
			throw;
		}
	}

	/// <summary>
	/// <inheritdoc cref="WebApplication.Run(string)"/>
	/// </summary>
	public static void Run(this WebApplication app, ILogger<Program> logger)
	{
		//ConfigurationCore(() =>
		//{
		//	app.Run();
		//}, "Run", logger);

		logger.LogInformation("Starting the application in {EnvironmentName} environment...", app.Environment.EnvironmentName);

		try
		{
			app.Run();
		}
		catch (Exception ex)
		{
			logger.LogCritical(ex, "Application could not run, see attached exception.");
			throw;
		}

		logger.LogInformation("Application terminated normally.");
	}
}
