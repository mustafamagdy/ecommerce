using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

// Spec to check if a BankStatement is already reconciled
public class BankStatementReconciledSpec : Specification<BankReconciliation>, ISingleResultSpecification
{
    public BankStatementReconciledSpec(Guid bankStatementId) =>
        Query.Where(br => br.BankStatementId == bankStatementId &&
                         (br.Status == ReconciliationStatus.Completed || br.Status == ReconciliationStatus.Closed));
}


public class CreateBankReconciliationHandler : IRequestHandler<CreateBankReconciliationRequest, Guid>
{
    private readonly IRepository<BankReconciliation> _reconciliationRepository;
    private readonly IReadRepository<BankAccount> _bankAccountRepository;
    private readonly IReadRepository<BankStatement> _bankStatementRepository;
    private readonly IReadRepository<Account> _glAccountRepository; // For fetching GL Account Balance
    private readonly IStringLocalizer<CreateBankReconciliationHandler> _localizer;
    private readonly ILogger<CreateBankReconciliationHandler> _logger;

    public CreateBankReconciliationHandler(
        IRepository<BankReconciliation> reconciliationRepository,
        IReadRepository<BankAccount> bankAccountRepository,
        IReadRepository<BankStatement> bankStatementRepository,
        IReadRepository<Account> glAccountRepository,
        IStringLocalizer<CreateBankReconciliationHandler> localizer,
        ILogger<CreateBankReconciliationHandler> logger)
    {
        _reconciliationRepository = reconciliationRepository;
        _bankAccountRepository = bankAccountRepository;
        _bankStatementRepository = bankStatementRepository;
        _glAccountRepository = glAccountRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateBankReconciliationRequest request, CancellationToken cancellationToken)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAsync(request.BankAccountId, cancellationToken);
        if (bankAccount == null)
            throw new NotFoundException(_localizer["Bank Account with ID {0} not found.", request.BankAccountId]);

        var bankStatement = await _bankStatementRepository.GetByIdAsync(request.BankStatementId, cancellationToken);
        if (bankStatement == null)
            throw new NotFoundException(_localizer["Bank Statement with ID {0} not found.", request.BankStatementId]);

        if (bankStatement.BankAccountId != request.BankAccountId)
            throw new ValidationException(_localizer["Bank Statement does not belong to the specified Bank Account."]);

        // Check if this statement is already reconciled in a completed/closed reconciliation
        bool alreadyReconciled = await _reconciliationRepository.AnyAsync(new BankStatementReconciledSpec(request.BankStatementId), cancellationToken);
        if (alreadyReconciled)
            throw new ConflictException(_localizer["Bank Statement {0} is already part of a completed reconciliation.", bankStatement.ReferenceNumber ?? bankStatement.Id.ToString()]);

        // Fetch the current balance of the GL Account linked to the BankAccount
        // This is a simplified placeholder. A real GL system would have ways to get balance at a specific date.
        // For now, using the current balance from the Account entity.
        var glAccount = await _glAccountRepository.GetByIdAsync(bankAccount.GLAccountId, cancellationToken);
        if (glAccount == null)
            throw new NotFoundException(_localizer["GL Account linked to Bank Account {0} not found.", bankAccount.AccountName]);
        decimal currentSystemBalance = glAccount.Balance; // Assuming Account.Balance holds the current balance

        var bankReconciliation = new BankReconciliation(
            bankAccountId: request.BankAccountId,
            reconciliationDate: request.ReconciliationDate, // This should be the StatementDate ideally
            bankStatementId: request.BankStatementId,
            statementBalance: bankStatement.ClosingBalance, // From the statement
            systemBalance: currentSystemBalance // From GL
            // Status defaults to Draft
        );

        await _reconciliationRepository.AddAsync(bankReconciliation, cancellationToken);

        _logger.LogInformation(_localizer["Bank Reconciliation created for Account '{0}' with Statement '{1}'."], bankAccount.AccountName, bankStatement.ReferenceNumber ?? bankStatement.Id.ToString());
        return bankReconciliation.Id;
    }
}
