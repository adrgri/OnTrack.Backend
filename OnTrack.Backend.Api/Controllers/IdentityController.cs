using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/identity")]
public sealed class IdentityController(SignInManager<ApplicationUser> signInManager)
	: Controller
{
	private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

	[Authorize]
	[HttpPost("logout")]
	public async Task<IActionResult> LogoutAsync()
	{
		await _signInManager.SignOutAsync();

		return Ok();
	}
}

// ---------------------------------------------------------------------------------------------------------------------- //

/*using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

using OnTrack.Backend.Api.Models;

using Task = System.Threading.Tasks.Task;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/identity")]
public sealed class IdentityController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserEmailStore<ApplicationUser> emailStore)
	: Controller
{
	private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly IUserEmailStore<ApplicationUser> _emailStore = emailStore;

	// Validate the email address using DataAnnotations like the UserValidator does when RequireUniqueEmail = true.
	private static readonly EmailAddressAttribute _emailAddressAttribute = new();

	[HttpPost("register")]
	public async Task<Results<Ok, ValidationProblem>> RegisterAsync([FromBody] RegisterRequest registration)
	{
		var email = registration.Email;

		if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
		{
			return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
		}

		ApplicationUser user = new();

		await _emailStore.SetUserNameAsync(user, email, CancellationToken.None);
		await emailStore.SetEmailAsync(user, email, CancellationToken.None);

		IdentityResult result = await userManager.CreateAsync(user, registration.Password);

		if (!result.Succeeded)
		{
			return CreateValidationProblem(result);
		}

		await SendConfirmationEmailAsync(user, userManager, context, email);
		return TypedResults.Ok();
	}

	[HttpGet("confirmEmail")]
	public async Task<Results<ContentHttpResult, UnauthorizedHttpResult>> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string code, [FromQuery] string? changedEmail)
	{
		var userManager = sp.GetRequiredService<UserManager<TUser>>();
		if (await userManager.FindByIdAsync(userId) is not { } user)
		{
			// We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
			return TypedResults.Unauthorized();
		}

		try
		{
			code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
		}
		catch (FormatException)
		{
			return TypedResults.Unauthorized();
		}

		IdentityResult result;

		if (string.IsNullOrEmpty(changedEmail))
		{
			result = await userManager.ConfirmEmailAsync(user, code);
		}
		else
		{
			// As with Identity UI, email and user name are one and the same. So when we update the email,
			// we need to update the user name.
			result = await userManager.ChangeEmailAsync(user, changedEmail, code);

			if (result.Succeeded)
			{
				result = await userManager.SetUserNameAsync(user, changedEmail);
			}
		}

		if (!result.Succeeded)
		{
			return TypedResults.Unauthorized();
		}

		return TypedResults.Text("Thank you for confirming your email.");
	}

	[HttpPost("resendConfirmationEmail")]
	public async Task<Ok> ResendConfirmationEmailAsync([FromBody] ResendConfirmationEmailRequest resendRequest)
	{
		var userManager = sp.GetRequiredService<UserManager<TUser>>();
		if (await userManager.FindByEmailAsync(resendRequest.Email) is not { } user)
		{
			return TypedResults.Ok();
		}

		await SendConfirmationEmailAsync(user, userManager, context, resendRequest.Email);
		return TypedResults.Ok();
	}

	[HttpPost("login")]
	public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> LoginAsync([FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies)
	{
		var signInManager = sp.GetRequiredService<SignInManager<TUser>>();

		var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
		var isPersistent = (useCookies == true) && (useSessionCookies != true);
		signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
		var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent, lockoutOnFailure: true);

		if (result.RequiresTwoFactor)
		{
			if (!string.IsNullOrEmpty(login.TwoFactorCode))
			{
				result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
			}
			else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
			{
				result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
			}
		}

		if (!result.Succeeded)
		{
			return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
		}

		// The signInManager already produced the needed response in the form of a cookie or bearer token.
		return TypedResults.Empty;
	}

	[HttpPost("refresh")]
	public async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>> RefreshAsync([FromBody] RefreshRequest refreshRequest)
	{
		var signInManager = sp.GetRequiredService<SignInManager<TUser>>();
		var refreshTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
		var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

		// Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
		if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
			timeProvider.GetUtcNow() >= expiresUtc ||
			await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not TUser user)

		{
			return TypedResults.Challenge();
		}

		var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
		return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
	}

	[HttpPost("forgotPassword")]
	public async Task<Results<Ok, ValidationProblem>> ForgotPasswordAsync([FromBody] ForgotPasswordRequest resetRequest)
	{
		var userManager = sp.GetRequiredService<UserManager<TUser>>();
		var user = await userManager.FindByEmailAsync(resetRequest.Email);

		if (user is not null && await userManager.IsEmailConfirmedAsync(user))
		{
			var code = await userManager.GeneratePasswordResetTokenAsync(user);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

			await emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
		}

		// Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
		// returned a 400 for an invalid code given a valid user email.
		return TypedResults.Ok();
	}

	[HttpPost("resetPassword")]
	public async Task<Results<Ok, ValidationProblem>> ResetPasswordAsync([FromBody] ResetPasswordRequest resetRequest)
	{
		var userManager = sp.GetRequiredService<UserManager<TUser>>();

		var user = await userManager.FindByEmailAsync(resetRequest.Email);

		if (user is null || !(await userManager.IsEmailConfirmedAsync(user)))
		{
			// Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
			// returned a 400 for an invalid code given a valid user email.
			return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));
		}

		IdentityResult result;
		try
		{
			var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
			result = await userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
		}
		catch (FormatException)
		{
			result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
		}

		if (!result.Succeeded)
		{
			return CreateValidationProblem(result);
		}

		return TypedResults.Ok();
	}

	[Authorize]
	[HttpGet("manage/2fa")]
	public async Task<Results<Ok<TwoFactorResponse>, ValidationProblem, NotFound>> Manage2faAsync([FromBody] TwoFactorRequest tfaRequest)
	{
		var signInManager = sp.GetRequiredService<SignInManager<TUser>>();
		var userManager = signInManager.UserManager;
		if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
		{
			return TypedResults.NotFound();
		}

		if (tfaRequest.Enable == true)
		{
			if (tfaRequest.ResetSharedKey)
			{
				return CreateValidationProblem("CannotResetSharedKeyAndEnable",
					"Resetting the 2fa shared key must disable 2fa until a 2fa token based on the new shared key is validated.");
			}
			else if (string.IsNullOrEmpty(tfaRequest.TwoFactorCode))
			{
				return CreateValidationProblem("RequiresTwoFactor",
					"No 2fa token was provided by the request. A valid 2fa token is required to enable 2fa.");
			}
			else if (!await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, tfaRequest.TwoFactorCode))
			{
				return CreateValidationProblem("InvalidTwoFactorCode",
					"The 2fa token provided by the request was invalid. A valid 2fa token is required to enable 2fa.");
			}

			await userManager.SetTwoFactorEnabledAsync(user, true);
		}
		else if (tfaRequest.Enable == false || tfaRequest.ResetSharedKey)
		{
			await userManager.SetTwoFactorEnabledAsync(user, false);
		}

		if (tfaRequest.ResetSharedKey)
		{
			await userManager.ResetAuthenticatorKeyAsync(user);
		}

		string[]? recoveryCodes = null;
		if (tfaRequest.ResetRecoveryCodes || (tfaRequest.Enable == true && await userManager.CountRecoveryCodesAsync(user) == 0))
		{
			var recoveryCodesEnumerable = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
			recoveryCodes = recoveryCodesEnumerable?.ToArray();
		}

		if (tfaRequest.ForgetMachine)
		{
			await signInManager.ForgetTwoFactorClientAsync();
		}

		var key = await userManager.GetAuthenticatorKeyAsync(user);
		if (string.IsNullOrEmpty(key))
		{
			await userManager.ResetAuthenticatorKeyAsync(user);
			key = await userManager.GetAuthenticatorKeyAsync(user);

			if (string.IsNullOrEmpty(key))
			{
				throw new NotSupportedException("The user manager must produce an authenticator key after reset.");
			}
		}

		return TypedResults.Ok(new TwoFactorResponse
		{
			SharedKey = key,
			RecoveryCodes = recoveryCodes,
			RecoveryCodesLeft = recoveryCodes?.Length ?? await userManager.CountRecoveryCodesAsync(user),
			IsTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
			IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user),
		});
	}

	[Authorize]
	[HttpGet("manage/info")]
	public async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> ManageInfoAsync()
	{
		var userManager = sp.GetRequiredService<UserManager<TUser>>();
		if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
		{
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
	}

	[Authorize]
	[HttpPost("manage/info")]
	public async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> ManageInfoAsync([FromBody] InfoRequest infoRequest)
	{
		var userManager = sp.GetRequiredService<UserManager<TUser>>();
		if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
		{
			return TypedResults.NotFound();
		}

		if (!string.IsNullOrEmpty(infoRequest.NewEmail) && !_emailAddressAttribute.IsValid(infoRequest.NewEmail))
		{
			return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(infoRequest.NewEmail)));
		}

		if (!string.IsNullOrEmpty(infoRequest.NewPassword))
		{
			if (string.IsNullOrEmpty(infoRequest.OldPassword))
			{
				return CreateValidationProblem("OldPasswordRequired",
					"The old password is required to set a new password. If the old password is forgotten, use /resetPassword.");
			}

			var changePasswordResult = await userManager.ChangePasswordAsync(user, infoRequest.OldPassword, infoRequest.NewPassword);
			if (!changePasswordResult.Succeeded)
			{
				return CreateValidationProblem(changePasswordResult);
			}
		}

		if (!string.IsNullOrEmpty(infoRequest.NewEmail))
		{
			var email = await userManager.GetEmailAsync(user);

			if (email != infoRequest.NewEmail)
			{
				await SendConfirmationEmailAsync(user, userManager, context, infoRequest.NewEmail, isChange: true);
			}
		}

		return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
	}

	[Authorize]
	[HttpPost("logout")]
	public async Task<IActionResult> LogoutAsync()
	{
		await _signInManager.SignOutAsync();

		return Ok();
	}

	private async Task SendConfirmationEmailAsync(ApplicationUser user, UserManager<ApplicationUser> userManager, HttpContext context, string email, bool isChange = false)
	{
		if (confirmEmailEndpointName is null)
		{
			throw new NotSupportedException("No email confirmation endpoint was registered!");
		}

		var code = isChange
			? await userManager.GenerateChangeEmailTokenAsync(user, email)
			: await userManager.GenerateEmailConfirmationTokenAsync(user);
		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

		var userId = await userManager.GetUserIdAsync(user);
		var routeValues = new RouteValueDictionary()
		{
			["userId"] = userId,
			["code"] = code,
		};

		if (isChange)
		{
			// This is validated by the /confirmEmail endpoint on change.
			routeValues.Add("changedEmail", email);
		}

		var confirmEmailUrl = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValues)
			?? throw new NotSupportedException($"Could not find endpoint named '{confirmEmailEndpointName}'.");

		await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
	}

	private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) => TypedResults.ValidationProblem(new Dictionary<string, string[]>
	{
		{ errorCode, [errorDescription] }
	});

	private static ValidationProblem CreateValidationProblem(IdentityResult result)
	{
		// We expect a single error code and description in the normal case.
		// This could be golfed with GroupBy and ToDictionary, but perf! :P
		Debug.Assert(!result.Succeeded);
		var errorDictionary = new Dictionary<string, string[]>(1);

		foreach (var error in result.Errors)
		{
			string[] newDescriptions;

			if (errorDictionary.TryGetValue(error.Code, out var descriptions))
			{
				newDescriptions = new string[descriptions.Length + 1];
				Array.Copy(descriptions, newDescriptions, descriptions.Length);
				newDescriptions[descriptions.Length] = error.Description;
			}
			else
			{
				newDescriptions = [error.Description];
			}

			errorDictionary[error.Code] = newDescriptions;
		}

		return TypedResults.ValidationProblem(errorDictionary);
	}

	private static async Task<InfoResponse> CreateInfoResponseAsync<TUser>(TUser user, UserManager<TUser> userManager)
		where TUser : class
	{
		return new()
		{
			Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
			IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
		};
	}

	// Wrap RouteGroupBuilder with a non-public type to avoid a potential future behavioral breaking change.
	private sealed class IdentityEndpointsConventionBuilder(RouteGroupBuilder inner) : IEndpointConventionBuilder
	{
		private IEndpointConventionBuilder InnerAsConventionBuilder => inner;

		public void Add(Action<EndpointBuilder> convention) => InnerAsConventionBuilder.Add(convention);
		public void Finally(Action<EndpointBuilder> finallyConvention) => InnerAsConventionBuilder.Finally(finallyConvention);
	}
}
*/