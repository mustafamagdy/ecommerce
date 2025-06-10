using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

using FSH.WebApi.Domain.Accounting; // Required for BankAccountByAccountNumberSpec

// Placeholder for BankAccountByNameSpec (if needed for unique name check, not strictly required by current request)
// public class BankAccountByNameSpec : Specification<BankAccount>, ISingleResultSpecification { ... }

public class CreateBankAccountHandler : IRequestHandler<CreateBankAccountRequest, Guid>
{
    private readonly IRepository<BankAccount> _bankAccountRepository;
    private readonly IReadRepository<Account> _glAccountRepository; // To validate GLAccountId
    private readonly IStringLocalizer<CreateBankAccountHandler> _localizer;
    private readonly ILogger<CreateBankAccountHandler> _logger;

    public CreateBankAccountHandler(
        IRepository<BankAccount> bankAccountRepository,
        IReadRepository<Account> glAccountRepository,
        IStringLocalizer<CreateBankAccountHandler> localizer,
        ILogger<CreateBankAccountHandler> logger)
    {
        _bankAccountRepository = bankAccountRepository;
        _glAccountRepository = glAccountRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateBankAccountRequest request, CancellationToken cancellationToken)
    {
        // 1. Validate GLAccountId
        var glAccount = await _glAccountRepository.GetByIdAsync(request.GLAccountId, cancellationToken);
        if (glAccount == null)
        {
            throw new NotFoundException(_localizer["GL Account with ID {0} not found.", request.GLAccountId]);
        }
        // Optionally, check if glAccount.IsActive or if it's a suitable type of account for a bank.
        if (!glAccount.IsActive) // Assuming Account entity has IsActive
        {
            // throw new ValidationException(_localizer["GL Account {0} is inactive.", glAccount.AccountNumber]);
        }


        // 2. Check for duplicate AccountNumber
        var existingByNumber = await _bankAccountRepository.FirstOrDefaultAsync(new BankAccountByAccountNumberSpec(request.AccountNumber), cancellationToken);
        if (existingByNumber != null)
        {
            throw new ConflictException(_localizer["Bank account with number {0} already exists.", request.AccountNumber]);
        }
        // Optional: Check for duplicate AccountName if that needs to be unique as well
        // var existingByName = await _bankAccountRepository.FirstOrDefaultAsync(new BankAccountByNameSpec(request.AccountName), cancellationToken);
        // if (existingByName != null) throw new ConflictException(_localizer["Bank account with name {0} already exists.", request.AccountName]);


        var bankAccount = new BankAccount(
            accountName: request.AccountName,
            accountNumber: request.AccountNumber,
            bankName: request.BankName,
            currency: request.Currency,
            glAccountId: request.GLAccountId,
            branchName: request.BranchName,
            isActive: request.IsActive
        );

        await _bankAccountRepository.AddAsync(bankAccount, cancellationToken);

        _logger.LogInformation(_localizer["Bank Account '{0}' (Number: {1}) created."], bankAccount.AccountName, bankAccount.AccountNumber);
        return bankAccount.Id;
    }
}
