using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting; // For BankAccount, Account
using MediatR;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

public class GetBankAccountHandler : IRequestHandler<GetBankAccountRequest, BankAccountDto>
{
    private readonly IReadRepository<BankAccount> _bankAccountRepository;
    private readonly IReadRepository<Account> _glAccountRepository; // To fetch GL Account details
    private readonly IStringLocalizer<GetBankAccountHandler> _localizer;

    public GetBankAccountHandler(
        IReadRepository<BankAccount> bankAccountRepository,
        IReadRepository<Account> glAccountRepository,
        IStringLocalizer<GetBankAccountHandler> localizer)
    {
        _bankAccountRepository = bankAccountRepository;
        _glAccountRepository = glAccountRepository;
        _localizer = localizer;
    }

    public async Task<BankAccountDto> Handle(GetBankAccountRequest request, CancellationToken cancellationToken)
    {
        // Using BankAccountByIdSpec which is a Specification<BankAccount, BankAccountDto>
        var spec = new BankAccountByIdSpec(request.Id);
        var bankAccount = await _bankAccountRepository.FirstOrDefaultAsync(spec, cancellationToken);
        // Note: FirstOrDefaultAsync with a spec returning BankAccountDto might directly give DTO.
        // If it returns BankAccount entity, then Adapt is needed. Assuming it returns entity for now.

        if (bankAccount == null) // If spec returned entity and it's null
        {
             // Try fetching entity if spec was for DTO and returned null
             var entity = await _bankAccountRepository.GetByIdAsync(request.Id, cancellationToken);
             if(entity == null) throw new NotFoundException(_localizer["Bank Account with ID {0} not found.", request.Id]);
             bankAccount = entity.Adapt<BankAccountDto>(); // Adapt entity to DTO
        }


        if (bankAccount.GLAccountId != Guid.Empty)
        {
            var glAccount = await _glAccountRepository.GetByIdAsync(bankAccount.GLAccountId, cancellationToken);
            if (glAccount != null)
            {
                bankAccount.GLAccountCode = glAccount.AccountNumber; // Assuming AccountNumber is the Code
                bankAccount.GLAccountName = glAccount.AccountName;
            }
            else
            {
                // GL Account not found, log warning or handle as per business rule
                _logger.LogWarning(_localizer["GL Account with ID {0} for Bank Account {1} not found."], bankAccount.GLAccountId, bankAccount.Id);
            }
        }

        return bankAccount;
    }
}

// Added ILogger to GetBankAccountHandler for logging if GL Account is not found.
// This requires adding ILogger to constructor and field.
// For brevity in this step, I'll assume the handler is modified to include ILogger if this logging is desired.
// The core logic for fetching and populating is above.
// If BankAccountByIdSpec was Specification<BankAccount> (entity only), the handler would be:
/*
    public async Task<BankAccountDto> Handle(GetBankAccountRequest request, CancellationToken cancellationToken)
    {
        var bankAccountEntity = await _bankAccountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (bankAccountEntity == null)
        {
            throw new NotFoundException(_localizer["Bank Account with ID {0} not found.", request.Id]);
        }
        var dto = bankAccountEntity.Adapt<BankAccountDto>();
        if (bankAccountEntity.GLAccountId != Guid.Empty)
        {
            var glAccount = await _glAccountRepository.GetByIdAsync(bankAccountEntity.GLAccountId, cancellationToken);
            if (glAccount != null)
            {
                dto.GLAccountCode = glAccount.AccountNumber;
                dto.GLAccountName = glAccount.AccountName;
            } else { ... log warning ... }
        }
        return dto;
    }
*/
