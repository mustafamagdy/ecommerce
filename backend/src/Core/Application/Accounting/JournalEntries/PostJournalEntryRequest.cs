using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.JournalEntries;

public class PostJournalEntryRequest : IRequest<Guid>
{
    [Required]
    public Guid JournalEntryId { get; set; }

    public PostJournalEntryRequest(Guid journalEntryId)
    {
        JournalEntryId = journalEntryId;
    }
}
