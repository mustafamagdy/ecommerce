using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Ledgers;

public class AccountLedgerDto
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<AccountLedgerEntryDto> Entries { get; set; } = new();
}
