using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting; // For BankStatementTransaction, BankReconciliation
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
// Assuming BankStatementTransactionByIdSpec is in BankStatements folder
using FSH.WebApi.Application.Accounting.BankStatements;


namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class MatchBankTransactionHandler : IRequestHandler<MatchBankTransactionRequest, Guid>
{
    private readonly IRepository<BankStatementTransaction> _statementTransactionRepository;
    private readonly IReadRepository<BankReconciliation> _reconciliationRepository;
    private readonly IReadRepository<BankStatement> _statementRepository; // To verify transaction belongs to statement of reconciliation
    private readonly IStringLocalizer<MatchBankTransactionHandler> _localizer;
    private readonly ILogger<MatchBankTransactionHandler> _logger;

    public MatchBankTransactionHandler(
        IRepository<BankStatementTransaction> statementTransactionRepository,
        IReadRepository<BankReconciliation> reconciliationRepository,
        IReadRepository<BankStatement> statementRepository,
        IStringLocalizer<MatchBankTransactionHandler> localizer,
        ILogger<MatchBankTransactionHandler> logger)
    {
        _statementTransactionRepository = statementTransactionRepository;
        _reconciliationRepository = reconciliationRepository;
        _statementRepository = statementRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(MatchBankTransactionRequest request, CancellationToken cancellationToken)
    {
        var reconciliation = await _reconciliationRepository.GetByIdAsync(request.BankReconciliationId, cancellationToken);
        if (reconciliation == null)
            throw new NotFoundException(_localizer["Bank Reconciliation with ID {0} not found.", request.BankReconciliationId]);

        if (reconciliation.Status != ReconciliationStatus.InProgress && reconciliation.Status != ReconciliationStatus.Draft)
            throw new ConflictException(_localizer["Bank Reconciliation {0} is not in a status that allows transaction matching (current: {1}).", request.BankReconciliationId, reconciliation.Status]);

        // Use the BankStatementTransactionByIdSpec (which should return the entity)
        // The spec was defined to return BankStatementTransactionDto, so we fetch entity directly.
        // var spec = new BankStatementTransactionByIdSpec(request.BankStatementTransactionId);
        var statementTransaction = await _statementTransactionRepository.GetByIdAsync(request.BankStatementTransactionId, cancellationToken);

        if (statementTransaction == null)
            throw new NotFoundException(_localizer["Bank Statement Transaction with ID {0} not found.", request.BankStatementTransactionId]);

        // Verify that the transaction belongs to the statement being reconciled
        var bankStatement = await _statementRepository.GetByIdAsync(reconciliation.BankStatementId, cancellationToken);
        if (bankStatement == null || statementTransaction.BankStatementId != bankStatement.Id)
            throw new ValidationException(_localizer["Transaction {0} does not belong to the statement {1} being reconciled.", request.BankStatementTransactionId, bankStatement?.ReferenceNumber ?? reconciliation.BankStatementId.ToString()]);


        if (request.IsMatched)
        {
            // Potentially validate SystemTransactionId and SystemTransactionType if they refer to real system transactions
            // e.g., fetch the system transaction to ensure it exists and its amount matches (this can be complex)

            statementTransaction.MarkAsReconciled(
                reconciliationId: request.BankReconciliationId,
                systemTransactionId: request.SystemTransactionId,
                systemTransactionType: request.SystemTransactionType
            );
            _logger.LogInformation(_localizer["Bank Statement Transaction ID {0} marked as reconciled for Reconciliation ID {1}."], request.BankStatementTransactionId, request.BankReconciliationId);
        }
        else
        {
            // If unmatching, ensure it was previously matched by this reconciliation
            if (statementTransaction.BankReconciliationId != request.BankReconciliationId && statementTransaction.IsReconciled)
            {
                // Log a warning or throw? If it was reconciled by another process, unmatching here might be an issue.
                _logger.LogWarning(_localizer["Bank Statement Transaction ID {0} was previously reconciled by a different process (ReconID: {1}). Unmatching it under current ReconID {2}."],
                    request.BankStatementTransactionId, statementTransaction.BankReconciliationId, request.BankReconciliationId);
            }
            statementTransaction.UnmarkAsReconciled();
            _logger.LogInformation(_localizer["Bank Statement Transaction ID {0} unmarked as reconciled for Reconciliation ID {1}."], request.BankStatementTransactionId, request.BankReconciliationId);
        }

        await _statementTransactionRepository.UpdateAsync(statementTransaction, cancellationToken);

        // Optional: Update BankReconciliation aggregate (e.g., recalculate differences, counts of matched items)
        // This could be complex and might be better handled by a separate "SummarizeReconciliation" step or event.
        // For now, this handler focuses on the BankStatementTransaction.

        return statementTransaction.Id;
    }
}
