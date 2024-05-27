using System.Net;
using System.Net.Mail;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using OnTrack.Backend.Api.Configuration;

namespace OnTrack.Backend.Api.Services;

public sealed class EmailSenderService<TUser> : IEmailSender<TUser>
	where TUser : class
{
	private readonly SmtpServicesOptions _options;

	// TODO: Add logging of send emails
	public EmailSenderService(IOptions<SmtpServicesOptions> options)
	{
		_options = options.Value;

		ServicePointManager.SecurityProtocol = _options.SecurityProtocol;
	}

	private MailAddress CreateSenderMailAddress()
	{
		return new MailAddress(_options.SenderEmail, _options.SenderDisplayName);
	}

	private SmtpClient CreateSmtpClient()
	{
		return new SmtpClient(_options.Host, _options.Port)
		{
			EnableSsl = _options.EnableSsl,
			Credentials = new NetworkCredential(_options.UserName, _options.Password)
		};
	}

	public async SysTask SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
	{
		using MailMessage mailMessage = new(CreateSenderMailAddress(), new(email))
		{
			Subject = "Confirm your email",
			Body = $"Please confirm your email by clicking <a href=\"{confirmationLink}\">here</a>.",
			IsBodyHtml = true
		};

		using SmtpClient smtpClient = CreateSmtpClient();

		await smtpClient.SendMailAsync(mailMessage);
	}

	public async SysTask SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
	{
		using MailMessage mailMessage = new(CreateSenderMailAddress(), new(email))
		{
			Subject = "Reset your password",
			Body = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.",
			IsBodyHtml = true
		};

		using SmtpClient smtpClient = CreateSmtpClient();

		await smtpClient.SendMailAsync(mailMessage);
	}

	public async SysTask SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
	{
		using MailMessage mailMessage = new(CreateSenderMailAddress(), new(email))
		{
			Subject = "Reset your password",
			// TODO: Replace the resetCode with a link to the reset password page just as in methods above?
			// Why did Microsoft's employees implemented this one method in the identity API differently???
			Body = $"Please reset your password using the following code: {resetCode}",
			IsBodyHtml = true
		};

		using SmtpClient smtpClient = CreateSmtpClient();

		await smtpClient.SendMailAsync(mailMessage);
	}
}
