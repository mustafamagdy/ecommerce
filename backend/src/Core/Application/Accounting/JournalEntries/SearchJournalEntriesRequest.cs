using FSH.WebApi.Application.Common.Models;
using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.JournalEntries;

public class SearchJournalEntriesRequest : PaginationFilter, IRequest<PaginationResponse<JournalEntryDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsPosted { get; set; }
    // Keyword is already part of PaginationFilter, so no need to redeclare.
    // public string? Keyword { get; set; }
}
