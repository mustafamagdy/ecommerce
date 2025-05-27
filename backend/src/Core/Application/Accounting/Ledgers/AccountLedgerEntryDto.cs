using System;

namespace FSH.WebApi.Application.Accounting.Ledgers;

public class AccountLedgerEntryDto
{
    public DateTime EntryDate { get; set; }
    public Guid JournalEntryId { get; set; }
    public string Description { get; set; } = default!;
    public string? ReferenceNumber { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal Balance { get; set; }
}
