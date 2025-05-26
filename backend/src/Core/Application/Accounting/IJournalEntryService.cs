using FSH.WebApi.Application.Accounting.Dtos;

namespace FSH.WebApi.Application.Accounting;

public interface IJournalEntryService : ITransientService
{
    Task<Guid> CreateJournalEntryAsync(CreateJournalEntryRequest request, CancellationToken cancellationToken);
    Task<JournalEntryDto> GetJournalEntryByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<JournalEntryDto> UpdateJournalEntryAsync(Guid id, UpdateJournalEntryRequest request, CancellationToken cancellationToken);
    Task<bool> PostJournalEntryAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> VoidJournalEntryAsync(Guid id, CancellationToken cancellationToken);
    Task<List<JournalEntryDto>> SearchJournalEntriesAsync(SearchJournalEntriesRequest request, CancellationToken cancellationToken); // Consider PaginationResult<JournalEntryDto>
}
