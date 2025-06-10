using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting; // For BankStatementTransaction
using FSH.WebApi.Application.Accounting.BankStatements; // For BankStatementTransactionDto
using MediatR;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using FSH.WebApi.Application.Common.Exceptions; // For NotFoundException

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class GetBankStatementTransactionsForReconciliationHandler
    : IRequestHandler<GetBankStatementTransactionsForReconciliationRequest, PaginationResponse<BankStatementTransactionDto>>
{
    private readonly IReadRepository<BankStatementTransaction> _transactionRepository;
    private readonly IReadRepository<BankReconciliation> _reconciliationRepository; // To get BankStatementId
    private readonly IStringLocalizer<GetBankStatementTransactionsForReconciliationHandler> _localizer;

    public GetBankStatementTransactionsForReconciliationHandler(
        IReadRepository<BankStatementTransaction> transactionRepository,
        IReadRepository<BankReconciliation> reconciliationRepository,
        IStringLocalizer<GetBankStatementTransactionsForReconciliationHandler> localizer)
    {
        _transactionRepository = transactionRepository;
        _reconciliationRepository = reconciliationRepository;
        _localizer = localizer;
    }

    public async Task<PaginationResponse<BankStatementTransactionDto>> Handle(
        GetBankStatementTransactionsForReconciliationRequest request, CancellationToken cancellationToken)
    {
        var reconciliation = await _reconciliationRepository.GetByIdAsync(request.BankReconciliationId, cancellationToken);
        if (reconciliation == null)
        {
            throw new NotFoundException(_localizer["Bank Reconciliation with ID {0} not found.", request.BankReconciliationId]);
        }

        var spec = new BankStatementTransactionsForReconciliationSpec(reconciliation.BankStatementId, request);

        var transactions = await _transactionRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _transactionRepository.CountAsync(spec, cancellationToken);

        // Assuming BankStatementTransactionDto.Type (string) is correctly mapped from BankTransactionType (enum) by Mapster.
        // If not, manual mapping would be needed here.
        // var dtos = transactions.Adapt<List<BankStatementTransactionDto>>();
        // Example if manual mapping for enum was needed:
        /*
        var dtos = new List<BankStatementTransactionDto>();
        foreach(var tx in transactions)
        {
            var dto = tx.Adapt<BankStatementTransactionDto>();
            dto.Type = tx.Type.ToString();
            // Populate SystemTransactionDetails if logic exists to fetch it based on SystemTransactionId/Type
            dtos.Add(dto);
        }
        */

        return new PaginationResponse<BankStatementTransactionDto>(
            transactions, // Assuming spec returns List<BankStatementTransactionDto>
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
