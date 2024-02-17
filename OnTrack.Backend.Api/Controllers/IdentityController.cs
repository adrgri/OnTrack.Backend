using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/identity")]
public sealed class IdentityController(SignInManager<AppUser> signInManager)
	: Controller
{
	private readonly SignInManager<AppUser> _signInManager = signInManager;

	[Authorize]
	[HttpPost("logout")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Logout()
	{
		// TODO: Add token and cookie invalidation here
		await _signInManager.SignOutAsync();

		return Ok();
	}
}
