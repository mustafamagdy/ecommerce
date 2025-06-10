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

public class UpdateBankReconciliationHandler : IRequestHandler<UpdateBankReconciliationRequest, Guid>
{
    private readonly IRepository<BankReconciliation> _reconciliationRepository;
    private readonly IStringLocalizer<UpdateBankReconciliationHandler> _localizer;
    private readonly ILogger<UpdateBankReconciliationHandler> _logger;

    public UpdateBankReconciliationHandler(
        IRepository<BankReconciliation> reconciliationRepository,
        IStringLocalizer<UpdateBankReconciliationHandler> localizer,
        ILogger<UpdateBankReconciliationHandler> logger)
    {
        _reconciliationRepository = reconciliationRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(UpdateBankReconciliationRequest request, CancellationToken cancellationToken)
    {
        var reconciliation = await _reconciliationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (reconciliation == null)
        {
            throw new NotFoundException(_localizer["Bank Reconciliation with ID {0} not found.", request.Id]);
        }

        // Use the domain entity's method to update, which contains business logic
        reconciliation.UpdateBalancesAndStatus(
            statementBalance: null, // Statement balance is usually fixed from the statement
            systemBalance: null,    // System balance typically updated by other processes or recalc summary
            status: request.Status,
            unclearedChecks: request.ManuallyAssignedUnclearedChecks,
            depositsInTransit: request.ManuallyAssignedDepositsInTransit
        );

        // Specific status transitions might be better as dedicated requests/handlers
        // e.g., CompleteReconciliationRequest, CloseReconciliationRequest
        // if (request.Status == ReconciliationStatus.Completed && reconciliation.Status != ReconciliationStatus.Completed)
        // {
        //     reconciliation.CompleteReconciliation();
        // }
        // else if (request.Status == ReconciliationStatus.Closed && reconciliation.Status != ReconciliationStatus.Closed)
        // {
        //     reconciliation.CloseReconciliation();
        // }

        await _reconciliationRepository.UpdateAsync(reconciliation, cancellationToken);

        _logger.LogInformation(_localizer["Bank Reconciliation ID {0} updated. Status: {1}."], reconciliation.Id, reconciliation.Status);
        return reconciliation.Id;
    }
}
