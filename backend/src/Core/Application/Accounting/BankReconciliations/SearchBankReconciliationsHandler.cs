using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting; // For BankReconciliation
using MediatR;
using Microsoft.Extensions.Localization; // If needed for logging/messages
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class SearchBankReconciliationsHandler : IRequestHandler<SearchBankReconciliationsRequest, PaginationResponse<BankReconciliationDto>>
{
    private readonly IReadRepository<BankReconciliation> _reconciliationRepository;

    public SearchBankReconciliationsHandler(IReadRepository<BankReconciliation> reconciliationRepository)
    {
        _reconciliationRepository = reconciliationRepository;
    }

    public async Task<PaginationResponse<BankReconciliationDto>> Handle(SearchBankReconciliationsRequest request, CancellationToken cancellationToken)
    {
        var spec = new BankReconciliationsBySearchFilterSpec(request);
        // This spec is Specification<BankReconciliation, BankReconciliationDto>
        // and includes BankAccount and BankStatement for populating names/references.

        var reconciliations = await _reconciliationRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _reconciliationRepository.CountAsync(spec, cancellationToken);

        // Similar to GetBankReconciliationHandler, if BankAccountName or BankStatementReference
        // are not populated by Mapster projection via the spec, they'd need manual setting.
        // For now, assuming the spec + Mapster handle this.
        // Also assuming Status enum to string is handled.

        return new PaginationResponse<BankReconciliationDto>(reconciliations, totalCount, request.PageNumber, request.PageSize);
    }
}
