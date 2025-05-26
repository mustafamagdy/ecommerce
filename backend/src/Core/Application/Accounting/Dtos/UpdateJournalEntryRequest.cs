namespace FSH.WebApi.Application.Accounting.Dtos;

public class UpdateJournalEntryRequest
{
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    // In a real scenario, you might want more granular control over transaction updates
    // e.g., AddTransactions, RemoveTransactions, UpdateTransactions
    // For simplicity here, we'll replace all transactions
    public List<TransactionRequest> Transactions { get; set; } = new();
}
