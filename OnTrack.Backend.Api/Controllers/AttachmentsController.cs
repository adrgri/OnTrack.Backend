using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Data;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("api/attachment")]
public class AttachmentsController(ILogger<StatusesController> logger, ApplicationDbContext context)
	: ControllerBase
{
	private readonly ILogger<StatusesController> _logger = logger;
	private readonly ApplicationDbContext _context = context;

	private bool AttachmentExists(AttachmentId id)
	{
		return _context.Attachments.Any(e => e.Id == id);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Attachment>> PostAttachment(Attachment attachment)
	{
		Status status = createStatusDto.ToDomainModel();

		_ = _context.Attachments.Add(attachment);
		_ = await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetAttachment), new { attachmentId = attachment.Id }, attachment);
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Attachment>>> GetAttachments()
	{
		return await _context.Attachments.ToListAsync();
	}

	[HttpGet("{attachmentId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Attachment>> GetAttachment(AttachmentId attachmentId)
	{
		Attachment? attachment = await _context.Attachments.FindAsync(attachmentId);

		return attachment switch
		{
			null => NotFound(),
			_ => attachment
		};
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutAttachment(Attachment attachment)
	{
		_context.Entry(attachment).State = EntityState.Modified;

		try
		{
			_ = await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException) when (AttachmentExists(attachment.Id) == false)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{attachmentId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteAttachment(AttachmentId attachmentId)
	{
		Attachment? attachment = await _context.Attachments.FindAsync(attachmentId);

		if (attachment is null)
		{
			return NotFound();
		}

		_ = _context.Attachments.Remove(attachment);

		// TODO Utwórz IDatabaseService i przenieś do niego tę logikę do niego
		try
		{
			_ = await _context.SaveChangesAsync();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the attachment with id {AttachmentId}.", attachmentId);

			return Conflict();
		}
	}
}
