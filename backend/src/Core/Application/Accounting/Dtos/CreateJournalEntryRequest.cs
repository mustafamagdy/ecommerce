namespace FSH.WebApi.Application.Accounting.Dtos;

public class CreateJournalEntryRequest
{
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<TransactionRequest> Transactions { get; set; } = new();
}
