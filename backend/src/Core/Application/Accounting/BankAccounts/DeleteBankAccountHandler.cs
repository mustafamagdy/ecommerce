using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting; // For BankAccount, BankStatement, BankReconciliation
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

// Spec to check for BankStatements linked to a BankAccount
public class BankStatementsByBankAccountIdSpec : Specification<BankStatement>, ISingleResultSpecification
{
    public BankStatementsByBankAccountIdSpec(Guid bankAccountId) =>
        Query.Where(bs => bs.BankAccountId == bankAccountId);
}

// Spec to check for BankReconciliations linked to a BankAccount
public class BankReconciliationsByBankAccountIdSpec : Specification<BankReconciliation>, ISingleResultSpecification
{
    public BankReconciliationsByBankAccountIdSpec(Guid bankAccountId) =>
        Query.Where(br => br.BankAccountId == bankAccountId);
}

public class DeleteBankAccountHandler : IRequestHandler<DeleteBankAccountRequest, Guid>
{
    private readonly IRepository<BankAccount> _bankAccountRepository;
    private readonly IReadRepository<BankStatement> _statementRepository;
    private readonly IReadRepository<BankReconciliation> _reconciliationRepository;
    private readonly IStringLocalizer<DeleteBankAccountHandler> _localizer;
    private readonly ILogger<DeleteBankAccountHandler> _logger;

    public DeleteBankAccountHandler(
        IRepository<BankAccount> bankAccountRepository,
        IReadRepository<BankStatement> statementRepository,
        IReadRepository<BankReconciliation> reconciliationRepository,
        IStringLocalizer<DeleteBankAccountHandler> localizer,
        ILogger<DeleteBankAccountHandler> logger)
    {
        _bankAccountRepository = bankAccountRepository;
        _statementRepository = statementRepository;
        _reconciliationRepository = reconciliationRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(DeleteBankAccountRequest request, CancellationToken cancellationToken)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (bankAccount == null)
        {
            throw new NotFoundException(_localizer["Bank Account with ID {0} not found.", request.Id]);
        }

        // Check for dependencies: BankStatements
        bool hasStatements = await _statementRepository.AnyAsync(new BankStatementsByBankAccountIdSpec(request.Id), cancellationToken);
        if (hasStatements)
        {
            throw new ConflictException(_localizer["Bank Account '{0}' has associated bank statements and cannot be deleted.", bankAccount.AccountName]);
        }

        // Check for dependencies: BankReconciliations
        bool hasReconciliations = await _reconciliationRepository.AnyAsync(new BankReconciliationsByBankAccountIdSpec(request.Id), cancellationToken);
        if (hasReconciliations)
        {
            throw new ConflictException(_localizer["Bank Account '{0}' has associated bank reconciliations and cannot be deleted.", bankAccount.AccountName]);
        }

        // Additional checks could include if this bank account is linked in any transactions (e.g. VendorPayments, CustomerPayments if directly linked)
        // or if it's the primary bank account for a company/branch, etc.

        await _bankAccountRepository.DeleteAsync(bankAccount, cancellationToken);

        _logger.LogInformation(_localizer["Bank Account '{0}' (ID: {1}) deleted."], bankAccount.AccountName, bankAccount.Id);
        return bankAccount.Id;
    }
}
