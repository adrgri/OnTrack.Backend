using Microsoft.AspNetCore.Mvc;

using OneOf;
using OneOf.Types;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.DataAccess;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;
using OnTrack.Backend.Api.Threading;
using OnTrack.Backend.Api.Validation;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/language")]
public sealed class LanguagesController(
	ILogger<LanguagesController> logger,
	IEntityAccessService<LanguageId, Language> languagesAccessService,
	IMapper<LanguageId, Language, LanguageDto> mapper,
	IAsyncCollectionValidator<LanguageId, OneOf<Language, EntityIdErrorsDescription<LanguageId>>> languageCollectionValidator)
	: GenericController<LanguageId, Language, LanguageDto, LanguagesController>(logger, languagesAccessService, mapper, languageCollectionValidator)
{
	protected override Task<OneOf<Language, ValidationFailure>> ConvertToNewDomainModel(LanguageDto entityDto)
	{
		OneOf<Language, ValidationFailure> output = ModelState.IsValid switch
		{
			true => Mapper.ToNewDomainModel(entityDto),
			false => new ValidationFailure()
		};

		return SysTask.FromResult(output);
	}

	protected override async Task<OneOf<Language, NotFound, ValidationFailure>> ConvertToNewDomainModel(LanguageId entityId, LanguageDto entityDto)
	{
		return (await ValidateEntityExistence(entityId, EntityCollectionValidator)).Match<OneOf<Language, NotFound, ValidationFailure>>(language =>
		{
			Mapper.ToExistingDomainModel(entityDto, language);

			return ModelState.IsValid ? language : new ValidationFailure();
		},
		(NotFound notFound) => notFound);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<LanguageDtoWithId>> PostLanguage(LanguageDto languageDto, CancellationToken cancellationToken)
	{
		return (await Post(languageDto, cancellationToken)).Match<ActionResult<LanguageDtoWithId>>(
			(Language language) => CreatedAtAction(nameof(GetLanguage), new { languageId = language.Id }, language),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet("{languageId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<LanguageDtoWithId>> GetLanguage(LanguageId languageId, CancellationToken cancellationToken)
	{
		return (await Get(languageId, cancellationToken)).Match<ActionResult<LanguageDtoWithId>>(
			(Language language) => new LanguageDtoWithId(language, Mapper),
			(NotFound _) => NotFound(),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<LanguageDtoWithId>>> GetLanguages(CancellationToken cancellationToken)
	{
		return (await GetAll(cancellationToken)).Match<ActionResult<IEnumerable<LanguageDtoWithId>>>(
			(List<Language> languagesList) => languagesList.ConvertAll(language => new LanguageDtoWithId(language, Mapper)),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> PutLanguage(LanguageDtoWithId languageDtoWithId, CancellationToken cancellationToken)
	{
		return (await Put(languageDtoWithId.Id, languageDtoWithId, cancellationToken)).Match(
			(Language _) => Ok(),
			(NotFound _) => NotFound(),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpDelete("{languageId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> DeleteLanguage(LanguageId languageId, CancellationToken cancellationToken)
	{
		return (await Delete(languageId, cancellationToken)).Match(
			(Success _) => Ok(),
			(NotFound _) => NotFound(),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}
}
