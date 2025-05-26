using FSH.WebApi.Domain.Accounting.Enums;

namespace FSH.WebApi.Application.Accounting.Dtos;

public class SearchJournalEntriesRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public JournalEntryStatus? Status { get; set; }
    public string? DescriptionKeyword { get; set; }
    public Guid? AccountId { get; set; } // To find entries related to a specific account
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
