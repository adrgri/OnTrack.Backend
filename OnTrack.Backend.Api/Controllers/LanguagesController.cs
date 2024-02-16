using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("api/language")]
public sealed class LanguagesController(IEntityAccessService<Language, LanguageId> languagesService, ILogger<StatusesController> logger)
	: ControllerBase
{
	private readonly IEntityAccessService<Language, LanguageId> _languagesService = languagesService;
	private readonly ILogger<StatusesController> _logger = logger;

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Language>> PostLanguage(LanguageDto createLanguageDto, [FromServices] IMapper<Language, LanguageId, LanguageDto> mapper)
	{
		Language language = mapper.ToNewDomainModel(createLanguageDto);

		await _languagesService.Add(language);
		await _languagesService.SaveChanges();

		return CreatedAtAction(nameof(GetLanguage), new { languageId = language.Id }, language);
	}

	[HttpGet("{languageId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Language>> GetLanguage(LanguageId languageId)
	{
		Language? language = await _languagesService.Find(languageId);

		return language switch
		{
			null => NotFound(),
			_ => language
		};
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Language>>> GetLanguages()
	{
		IEnumerable<Language> languages = await _languagesService.GetAll();

		return languages.ToList();
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutLanguage(LanguageId languageId, LanguageDto languageDto, [FromServices] IMapper<Language, LanguageId, LanguageDto> mapper)
	{
		Language? language = await _languagesService.Find(languageId);

		if (language is null)
		{
			return NotFound();
		}

		mapper.ToExistingDomainModel(languageDto, language);

		await _languagesService.Update(language);

		try
		{
			await _languagesService.SaveChanges();
		}
		catch (DbUpdateConcurrencyException)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{languageId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteLanguage(LanguageId languageId)
	{
		Language? language = await _languagesService.Find(languageId);

		if (language is null)
		{
			return NotFound();
		}

		await _languagesService.Remove(language);

		try
		{
			await _languagesService.SaveChanges();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the language with id {LanguageId}.", languageId);

			return Conflict();
		}
	}
}
