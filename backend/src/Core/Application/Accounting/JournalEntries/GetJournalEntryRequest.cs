using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.JournalEntries;

public class GetJournalEntryRequest : IRequest<JournalEntryDto>
{
    [Required]
    public Guid Id { get; set; }

    public GetJournalEntryRequest(Guid id)
    {
        Id = id;
    }
}
