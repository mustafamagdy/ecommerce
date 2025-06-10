using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting; // For BankAccount, Account, BankAccountByAccountNumberSpec
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

public class UpdateBankAccountHandler : IRequestHandler<UpdateBankAccountRequest, Guid>
{
    private readonly IRepository<BankAccount> _bankAccountRepository;
    private readonly IReadRepository<Account> _glAccountRepository; // To validate GLAccountId if changed
    private readonly IStringLocalizer<UpdateBankAccountHandler> _localizer;
    private readonly ILogger<UpdateBankAccountHandler> _logger;

    public UpdateBankAccountHandler(
        IRepository<BankAccount> bankAccountRepository,
        IReadRepository<Account> glAccountRepository,
        IStringLocalizer<UpdateBankAccountHandler> localizer,
        ILogger<UpdateBankAccountHandler> logger)
    {
        _bankAccountRepository = bankAccountRepository;
        _glAccountRepository = glAccountRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(UpdateBankAccountRequest request, CancellationToken cancellationToken)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (bankAccount == null)
        {
            throw new NotFoundException(_localizer["Bank Account with ID {0} not found.", request.Id]);
        }

        // Validate GLAccountId if changed
        if (request.GLAccountId.HasValue && request.GLAccountId.Value != bankAccount.GLAccountId)
        {
            var glAccount = await _glAccountRepository.GetByIdAsync(request.GLAccountId.Value, cancellationToken);
            if (glAccount == null)
            {
                throw new NotFoundException(_localizer["GL Account with ID {0} not found.", request.GLAccountId.Value]);
            }
            // if (!glAccount.IsActive) throw new ValidationException(_localizer["GL Account {0} is inactive.", glAccount.AccountNumber]);
        }

        // Check for duplicate AccountNumber if it's being changed
        if (request.AccountNumber is not null && !bankAccount.AccountNumber.Equals(request.AccountNumber, StringComparison.OrdinalIgnoreCase))
        {
            var existingByNumber = await _bankAccountRepository.FirstOrDefaultAsync(new BankAccountByAccountNumberSpec(request.AccountNumber), cancellationToken);
            if (existingByNumber != null && existingByNumber.Id != bankAccount.Id)
            {
                throw new ConflictException(_localizer["Bank account with number {0} already exists.", request.AccountNumber]);
            }
        }
        // Optional: Check for duplicate AccountName if that needs to be unique as well and is being changed

        bankAccount.Update(
            accountName: request.AccountName,
            accountNumber: request.AccountNumber,
            bankName: request.BankName,
            currency: request.Currency,
            glAccountId: request.GLAccountId,
            branchName: request.BranchName,
            isActive: request.IsActive
        );

        await _bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);

        _logger.LogInformation(_localizer["Bank Account '{0}' (ID: {1}) updated."], bankAccount.AccountName, bankAccount.Id);
        return bankAccount.Id;
    }
}
