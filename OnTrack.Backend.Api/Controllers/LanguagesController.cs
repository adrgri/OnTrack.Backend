using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Data;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("api/language")]
public sealed class LanguagesController(ILogger<StatusesController> logger, ApplicationDbContext context)
	: ControllerBase
{
	private readonly ILogger<StatusesController> _logger = logger;
	private readonly ApplicationDbContext _context = context;

	private bool LanguageExists(LanguageId id)
	{
		return _context.Languages.Any(e => e.Id == id);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Language>> PostLanguage(CreateLanguageDto createLanguageDto)
	{
		Language language = createLanguageDto.ToDomainModel();

		_ = _context.Languages.Add(language);
		_ = await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetLanguage), new { languageId = language.Id }, language);
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Language>>> GetLanguages()
	{
		return await _context.Languages.ToListAsync();
	}

	[HttpGet("{languageId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Language>> GetLanguage(LanguageId languageId)
	{
		Language? language = await _context.Languages.FindAsync(languageId);

		return language switch
		{
			null => NotFound(),
			_ => language
		};
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutLanguage(Language language)
	{
		_context.Entry(language).State = EntityState.Modified;

		try
		{
			_ = await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException) when (LanguageExists(language.Id) == false)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{languageId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteLanguage(LanguageId languageId)
	{
		Language? language = await _context.Languages.FindAsync(languageId);

		if (language is null)
		{
			return NotFound();
		}

		_ = _context.Languages.Remove(language);

		// TODO Utwórz IDatabaseService i przenieś do niego tę logikę do niego
		try
		{
			_ = await _context.SaveChangesAsync();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the language with id {LanguageId}.", languageId);

			return Conflict();
		}
	}
}
