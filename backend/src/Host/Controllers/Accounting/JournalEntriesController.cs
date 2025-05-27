using FSH.WebApi.Application.Accounting.JournalEntries;
using FSH.WebApi.Application.Common.Models; // For PaginationResponse
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using NSwag.Annotations; // For OpenApiOperation

namespace FSH.WebApi.Host.Controllers.Accounting;

public class JournalEntriesController : VersionedApiController
{
    private readonly IMediator _mediator;

    public JournalEntriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("journal-entries")]
    [OpenApiOperation("Create a new journal entry.", "")]
    // [MustHavePermission(FSHAction.Create, FSHResource.JournalEntries)]
    public async Task<ActionResult<Guid>> CreateJournalEntryAsync(CreateJournalEntryRequest request)
    {
        var journalEntryId = await _mediator.Send(request);
        return Ok(journalEntryId);
    }

    [HttpPost("journal-entries/{id:guid}/post")]
    [OpenApiOperation("Post a journal entry.", "")]
    // [MustHavePermission(FSHAction.Update, FSHResource.JournalEntries)] // Or a custom "Post" action
    public async Task<ActionResult<Guid>> PostJournalEntryAsync(Guid id)
    {
        // Assuming PostJournalEntryRequest takes the ID in its constructor or a property
        var journalEntryId = await _mediator.Send(new PostJournalEntryRequest(id));
        return Ok(journalEntryId);
    }

    [HttpGet("journal-entries/{id:guid}")]
    [OpenApiOperation("Get journal entry details by ID.", "")]
    // [MustHavePermission(FSHAction.View, FSHResource.JournalEntries)]
    public async Task<ActionResult<JournalEntryDto>> GetJournalEntryByIdAsync(Guid id)
    {
        var journalEntryDto = await _mediator.Send(new GetJournalEntryRequest(id));
        return Ok(journalEntryDto);
    }

    [HttpPost("journal-entries/search")]
    [OpenApiOperation("Search journal entries using available filters.", "")]
    // [MustHavePermission(FSHAction.Search, FSHResource.JournalEntries)]
    public async Task<ActionResult<PaginationResponse<JournalEntryDto>>> SearchJournalEntriesAsync(SearchJournalEntriesRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }
}
