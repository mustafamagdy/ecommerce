using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting; // For BankReconciliation, BankAccount, BankStatement
using MediatR;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class GetBankReconciliationHandler : IRequestHandler<GetBankReconciliationRequest, BankReconciliationDto>
{
    private readonly IReadRepository<BankReconciliation> _reconciliationRepository;
    private readonly IStringLocalizer<GetBankReconciliationHandler> _localizer;
    // BankAccount and BankStatement repositories are not needed if spec includes them and Mapster handles projection.

    public GetBankReconciliationHandler(
        IReadRepository<BankReconciliation> reconciliationRepository,
        IStringLocalizer<GetBankReconciliationHandler> localizer)
    {
        _reconciliationRepository = reconciliationRepository;
        _localizer = localizer;
    }

    public async Task<BankReconciliationDto> Handle(GetBankReconciliationRequest request, CancellationToken cancellationToken)
    {
        var spec = new BankReconciliationByIdWithDetailsSpec(request.Id);
        // This spec is Specification<BankReconciliation, BankReconciliationDto>
        // It should include BankAccount and BankStatement for name/reference population.
        var reconciliationDto = await _reconciliationRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (reconciliationDto == null)
        {
            throw new NotFoundException(_localizer["Bank Reconciliation with ID {0} not found.", request.Id]);
        }

        // If BankAccountName or BankStatementReference are not populated by Mapster projection via the spec,
        // they would need to be fetched and set manually here.
        // However, the spec was designed to include BankAccount and BankStatement.
        // Example if manual population was needed:
        // if (string.IsNullOrEmpty(reconciliationDto.BankAccountName) && reconciliationEntity.BankAccount != null)
        // {
        //     reconciliationDto.BankAccountName = $"{reconciliationEntity.BankAccount.BankName} - {reconciliationEntity.BankAccount.AccountNumber}";
        // }
        // if (string.IsNullOrEmpty(reconciliationDto.BankStatementReference) && reconciliationEntity.BankStatement != null)
        // {
        //     reconciliationDto.BankStatementReference = reconciliationEntity.BankStatement.ReferenceNumber ?? reconciliationEntity.BankStatement.Id.ToString();
        // }
        // The Status enum to string mapping should also be handled by Mapster.

        return reconciliationDto;
    }
}
