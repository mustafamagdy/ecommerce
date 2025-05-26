using FSH.WebApi.Application.Accounting.Dtos;

namespace FSH.WebApi.Application.Accounting;

public interface IAccountService : ITransientService
{
    Task<Guid> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken);
    Task<AccountDto> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAccountAsync(Guid id, CancellationToken cancellationToken);
    Task<List<AccountDto>> SearchAccountsAsync(SearchAccountsRequest request, CancellationToken cancellationToken); // Consider PaginationResult<AccountDto>
}
