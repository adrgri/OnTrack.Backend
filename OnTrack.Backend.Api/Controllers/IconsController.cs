//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//using OnTrack.Backend.Api.Dto;
//using OnTrack.Backend.Api.Models;
//using OnTrack.Backend.Api.Services;

//namespace OnTrack.Backend.Api.Controllers;

//[ApiController, Route("/api/icon")]
//public class IconsController(IEntityAccessService<Icon, IconId> iconsService, ILogger<StatusesController> logger)
//	: ControllerBase
//{
//	private readonly IEntityAccessService<Icon, IconId> _iconsService = iconsService;
//	private readonly ILogger<StatusesController> _logger = logger;

//	[HttpPost]
//	[ProducesResponseType(StatusCodes.Status201Created)]
//	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status400BadRequest)]
//	public async Task<ActionResult<Icon>> PostIcon(Icon icon)
//	{
//		//Status status = createStatusDto.ToDomainModel();

//		await _iconsService.Add(icon);
//		await _iconsService.SaveChanges();

//		return CreatedAtAction(nameof(GetIcon), new { iconId = icon.Id }, icon);
//	}

//	[HttpGet("{iconId}")]
//	[ProducesResponseType(StatusCodes.Status200OK)]
//	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
//	public async Task<ActionResult<Icon>> GetIcon(IconId iconId)
//	{
//		Icon? icon = await _iconsService.Find(iconId);

//		return icon switch
//		{
//			null => NotFound(),
//			_ => icon
//		};
//	}

//	[HttpGet]
//	[ProducesResponseType(StatusCodes.Status200OK)]
//	public async Task<ActionResult<IEnumerable<Icon>>> GetIcons()
//	{
//		IEnumerable<Icon> icons = await _iconsService.GetAll();

//		return icons.ToList();
//	}

//	[HttpPut]
//	[ProducesResponseType(StatusCodes.Status200OK)]
//	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
//	public async Task<IActionResult> PutIcon(Icon icon)
//	{
//		_context.Entry(icon).State = EntityState.Modified;

//		try
//		{
//			_ = await _context.SaveChangesAsync();
//		}
//		catch (DbUpdateConcurrencyException) when (IconExists(icon.Id) == false)
//		{
//			return NotFound();
//		}

//		return Ok();
//	}

//	[HttpDelete("{iconId}")]
//	[ProducesResponseType(StatusCodes.Status200OK)]
//	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
//	public async Task<IActionResult> DeleteIcon(IconId iconId)
//	{
//		Icon? icon = await _context.Icons.FindAsync(iconId);

//		if (icon is null)
//		{
//			return NotFound();
//		}

//		_ = _context.Icons.Remove(icon);

//		// TODO Utwórz IDatabaseService i przenieś do niego tę logikę do niego
//		try
//		{
//			_ = await _context.SaveChangesAsync();

//			return Ok();
//		}
//		catch (DbUpdateConcurrencyException ex)
//		{
//			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the icon with id {IconId}.", iconId);

//			return Conflict();
//		}
//	}
//}
