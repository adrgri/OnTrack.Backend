//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//using OnTrack.Backend.Api.Application.Mappings;
//using OnTrack.Backend.Api.Dto;
//using OnTrack.Backend.Api.Models;
//using OnTrack.Backend.Api.Services;

//namespace OnTrack.Backend.Api.Controllers;

//[ApiController, Route("/api/attachment")]
//public class AttachmentsController(ILogger<StatusesController> logger, IEntityAccessService<Attachment, AttachmentId> attachmentsService)
//	: ControllerBase
//{
//	private readonly IEntityAccessService<Attachment, AttachmentId> _attachmentsService = attachmentsService;
//	private readonly ILogger<StatusesController> _logger = logger;

//	[HttpPost]
//	[ProducesResponseType(StatusCodes.Status201Created)]
//	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status400BadRequest)]
//	public async Task<ActionResult<Attachment>> PostAttachment(AttachmentDto attachmentDto, [FromServices] IMapper<Attachment, AttachmentId, AttachmentDto> mapper)
//	{
//		Attachment attachment = mapper.ToNewDomainModel(attachmentDto);

//		await _attachmentsService.Add(attachment);
//		await _attachmentsService.SaveChanges();

//		return CreatedAtAction(nameof(GetAttachment), new { attachmentId = attachment.Id }, attachment);
//	}

//	[HttpGet("{attachmentId}")]
//	[ProducesResponseType(StatusCodes.Status200OK)]
//	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
//	public async Task<ActionResult<Attachment>> GetAttachment(AttachmentId attachmentId)
//	{
//		Attachment? attachment = await _attachmentsService.Find(attachmentId);

//		return attachment switch
//		{
//			null => NotFound(),
//			_ => attachment
//		};
//	}

//	[HttpGet]
//	[ProducesResponseType(StatusCodes.Status200OK)]
//	public async Task<ActionResult<IEnumerable<Attachment>>> GetAttachments()
//	{
//		IEnumerable<Attachment> attachments = await _attachmentsService.GetAll();

//		return attachments.ToList();
//	}

//	[HttpPut]
//	[ProducesResponseType(StatusCodes.Status200OK)]
//	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
//	public async Task<IActionResult> PutAttachment(AttachmentId attachmentId, AttachmentDto attachmentDto, [FromServices] IMapper<Attachment, AttachmentId, AttachmentDto> mapper)
//	{
//		Attachment? attachment = await _attachmentsService.Find(attachmentId);

//		if (attachment is null)
//		{
//			return NotFound();
//		}

//		mapper.ToExistingDomainModel(attachmentDto, attachment);

//		await _attachmentsService.Update(attachment);

//		try
//		{
//			await _attachmentsService.SaveChanges();
//		}
//		catch (DbUpdateConcurrencyException)
//		{
//			return Conflict();
//		}

//		return Ok();
//	}

//	[HttpDelete("{attachmentId}")]
//	[ProducesResponseType(StatusCodes.Status200OK)]
//	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
//	public async Task<IActionResult> DeleteAttachment(AttachmentId attachmentId)
//	{
//		Attachment? attachment = await _attachmentsService.Find(attachmentId);

//		if (attachment is null)
//		{
//			return NotFound();
//		}

//		await _attachmentsService.Remove(attachment);

//		// TODO Utwórz IDatabaseService i przenieś do niego tę logikę do niego
//		try
//		{
//			await _attachmentsService.SaveChanges();

//			return Ok();
//		}
//		catch (DbUpdateConcurrencyException ex)
//		{
//			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the attachment with id {AttachmentId}.", attachmentId);

//			return Conflict();
//		}
//	}
//}
