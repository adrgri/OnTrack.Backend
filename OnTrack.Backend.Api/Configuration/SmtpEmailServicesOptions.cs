using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Configuration;

public class SmtpEmailServicesOptions : IOptionsSection
{
	[JsonIgnore]
	public static string SectionKey => "Smtp";

	[JsonIgnore]
	public bool Enabled { get; set; }

	[RequiredWhenSmtpEmailServicesEnabled]
	public string Host { get; set; }

	[RequiredWhenSmtpEmailServicesEnabled]
	public int Port { get; set; }

	[RequiredWhenSmtpEmailServicesEnabled]
	public bool EnableSsl { get; set; }

	[RequiredWhenSmtpEmailServicesEnabled]
	public SecurityProtocolType SecurityProtocol { get; set; } = SecurityProtocolType.Tls12;

	[RequiredWhenSmtpEmailServicesEnabled, JsonIgnore]
	public string UserName { get; set; }

	[RequiredWhenSmtpEmailServicesEnabled, JsonIgnore]
	public string Password { get; set; }

	[RequiredWhenSmtpEmailServicesEnabled]
	public string SenderEmail { get; set; }

	public string? SenderDisplayName { get; set; }
}

// I have not tested how this attribute will behave when applied to fields or parameters, so I removed those from the "validOn" attribute target types
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class RequiredWhenAttribute<T>(Predicate<T> shouldValidate)
	: RequiredAttribute
{
	protected Predicate<T> ShouldValidate { get; init; } = shouldValidate;

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		T obj = (T)validationContext.ObjectInstance;

		return ShouldValidate(obj) ? base.IsValid(value, validationContext) : ValidationResult.Success;
	}
}

public sealed class RequiredWhenSmtpEmailServicesEnabledAttribute()
	: RequiredWhenAttribute<SmtpEmailServicesOptions>(options => options.Enabled);
