using System.Net;
using System.Text.Json.Serialization;

using OnTrack.Backend.Api.ComponentModel.DataAnnotations;

namespace OnTrack.Backend.Api.Configuration;

public class SmtpEmailServicesOptions : IOptionsSection
{
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
