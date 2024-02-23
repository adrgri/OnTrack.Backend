//using Microsoft.AspNetCore.Antiforgery;
//using Microsoft.AspNetCore.Mvc;

//namespace OnTrack.Backend.Api.Controllers;

//[ApiController, Route("/api/security/antiforgery/")]
//public class AntiforgeryController
//	: ControllerBase
//{
//	[HttpGet("token")]
//	[ProducesResponseType(StatusCodes.Status204NoContent)]
//	public ActionResult<string> GetAntiforgeryToken([FromServices] IAntiforgery antiforgeryService)
//	{
//		_ = antiforgeryService.GetAndStoreTokens(HttpContext);

//		return NoContent();
//	}
//}
