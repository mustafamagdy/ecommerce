using FSH.WebApi.Application.Accounting;
using FSH.WebApi.Application.Accounting.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FSH.WebApi.Host.Controllers.Accounting;

[Route("api/v1/accounting/journalentries")]
public class JournalEntriesController : VersionedApiController
{
    private readonly IJournalEntryService _journalEntryService;

    public JournalEntriesController(IJournalEntryService journalEntryService) =>
        _journalEntryService = journalEntryService;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateJournalEntryAsync(CreateJournalEntryRequest request, CancellationToken cancellationToken)
    {
        var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetJournalEntryByIdAsync), new { id = journalEntryId }, journalEntryId);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JournalEntryDto>> GetJournalEntryByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var journalEntry = await _journalEntryService.GetJournalEntryByIdAsync(id, cancellationToken);
        return Ok(journalEntry);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JournalEntryDto>> UpdateJournalEntryAsync(Guid id, UpdateJournalEntryRequest request, CancellationToken cancellationToken)
    {
        var updatedJournalEntry = await _journalEntryService.UpdateJournalEntryAsync(id, request, cancellationToken);
        return Ok(updatedJournalEntry);
    }

    [HttpPost("{id:guid}/post")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // For invalid operations, e.g., already posted
    public async Task<ActionResult<bool>> PostJournalEntryAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _journalEntryService.PostJournalEntryAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/void")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // For invalid operations, e.g., not posted
    public async Task<ActionResult<bool>> VoidJournalEntryAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _journalEntryService.VoidJournalEntryAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<JournalEntryDto>>> SearchJournalEntriesAsync(SearchJournalEntriesRequest request, CancellationToken cancellationToken)
    {
        var journalEntries = await _journalEntryService.SearchJournalEntriesAsync(request, cancellationToken);
        return Ok(journalEntries);
    }
}
