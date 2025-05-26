using FSH.WebApi.Domain.Accounting.Enums;

namespace FSH.WebApi.Application.Accounting.Dtos;

public class JournalEntryDto
{
    public Guid Id { get; set; }
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public JournalEntryStatus Status { get; set; }
    public List<TransactionDto> Transactions { get; set; } = new(); // Initialize to prevent null reference
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}

public class TransactionDto // Can be in its own file if preferred, placing here for brevity
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty; // For display purposes
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
