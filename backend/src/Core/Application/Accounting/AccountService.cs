using FSH.WebApi.Application.Accounting.Dtos;
using FSH.WebApi.Application.Common.Interfaces; // Assuming IRepository and other common interfaces are here
using FSH.WebApi.Domain.Accounting; // For Account entity
using FSH.WebApi.Application.Common.Exceptions; // For NotFoundException
using FSH.WebApi.Application.Common.Persistence; // For IRepository
using Mapster; // For mapping if used, or manual mapping

namespace FSH.WebApi.Application.Accounting;

public class AccountService : IAccountService
{
    private readonly IRepository<Account> _accountRepository;
    // private readonly IStringLocalizer<AccountService> _localizer; // Example for localization

    public AccountService(IRepository<Account> accountRepository /*, IStringLocalizer<AccountService> localizer*/)
    {
        _accountRepository = accountRepository;
        // _localizer = localizer;
    }

    public async Task<Guid> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        // Basic validation (can be moved to a validator)
        if (string.IsNullOrWhiteSpace(request.AccountNumber) || string.IsNullOrWhiteSpace(request.AccountName))
        {
            throw new ArgumentException("Account number and name are required.");
        }

        var account = new Account(request.AccountNumber, request.AccountName, request.AccountType, request.Balance);

        await _accountRepository.AddAsync(account, cancellationToken);
        // In a real scenario, consider Domain Events for integration

        return account.Id;
    }

    public async Task<AccountDto> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);

        _ = account ?? throw new NotFoundException($"Account with ID {id} not found.");

        // Manual mapping for now, or use Mapster/AutoMapper
        return new AccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            AccountType = account.AccountType,
            Balance = account.Balance,
            IsActive = account.IsActive,
            CreatedOn = account.CreatedOn,
            LastModifiedOn = account.LastModifiedOn
        };
    }

    public async Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);

        _ = account ?? throw new NotFoundException($"Account with ID {id} not found.");

        // Basic validation
        if (string.IsNullOrWhiteSpace(request.AccountNumber) || string.IsNullOrWhiteSpace(request.AccountName))
        {
            throw new ArgumentException("Account number and name are required for update.");
        }

        account.UpdateAccountDetails(request.AccountNumber, request.AccountName, request.AccountType);
        if (request.IsActive)
        {
            account.Activate();
        }
        else
        {
            account.Deactivate();
        }

        await _accountRepository.UpdateAsync(account, cancellationToken);

        // Manual mapping
        return new AccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            AccountType = account.AccountType,
            Balance = account.Balance,
            IsActive = account.IsActive,
            CreatedOn = account.CreatedOn,
            LastModifiedOn = account.LastModifiedOn
        };
    }

    public async Task<bool> DeleteAccountAsync(Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);

        _ = account ?? throw new NotFoundException($"Account with ID {id} not found.");

        // Consider soft delete or business logic before actual deletion
        // For example, an account with a non-zero balance might not be deletable directly.
        if (account.Balance != 0)
        {
            throw new InvalidOperationException("Cannot delete account with non-zero balance. Please adjust balance or deactivate the account.");
        }
        if (account.IsActive)
        {
             // Or throw new InvalidOperationException("Cannot delete an active account. Please deactivate it first.");
             account.Deactivate(); // Deactivate before deleting
        }

        await _accountRepository.DeleteAsync(account, cancellationToken);
        return true;
    }

    public async Task<List<AccountDto>> SearchAccountsAsync(SearchAccountsRequest request, CancellationToken cancellationToken)
    {
        // This is a placeholder. Real implementation would use IReadRepository and Specification pattern
        // or build a dynamic query.
        var allAccounts = await _accountRepository.ListAsync(cancellationToken); // Not efficient for large datasets

        var filteredAccounts = allAccounts
            .Where(a => (request.AccountNumber == null || a.AccountNumber.Contains(request.AccountNumber, StringComparison.OrdinalIgnoreCase)) &&
                        (request.AccountName == null || a.AccountName.Contains(request.AccountName, StringComparison.OrdinalIgnoreCase)) &&
                        (!request.AccountType.HasValue || a.AccountType == request.AccountType.Value) &&
                        (!request.IsActive.HasValue || a.IsActive == request.IsActive.Value))
            .ToList();

        // Manual mapping
        return filteredAccounts.Select(account => new AccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            AccountType = account.AccountType,
            Balance = account.Balance,
            IsActive = account.IsActive,
            CreatedOn = account.CreatedOn, // This part was already correct in the file content shown
            LastModifiedOn = account.LastModifiedOn // This part was also correct
        })
        // Basic pagination (in-memory, not ideal for DB)
        .Skip((request.PageNumber - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToList();
    }
}
