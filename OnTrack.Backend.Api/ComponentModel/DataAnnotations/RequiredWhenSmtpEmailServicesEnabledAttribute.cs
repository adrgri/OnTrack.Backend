using OnTrack.Backend.Api.Configuration;

namespace OnTrack.Backend.Api.ComponentModel.DataAnnotations;

public sealed class RequiredWhenSmtpEmailServicesEnabledAttribute()
	: RequiredWhenAttribute<SmtpServicesOptions>(options => options.Enabled, "the SMTP email services are enabled");
