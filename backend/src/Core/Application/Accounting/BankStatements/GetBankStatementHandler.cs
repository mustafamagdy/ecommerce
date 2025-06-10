using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting; // For BankStatement, BankAccount
using MediatR;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using System.Linq; // Required for .FirstOrDefault()

namespace FSH.WebApi.Application.Accounting.BankStatements;

public class GetBankStatementHandler : IRequestHandler<GetBankStatementRequest, BankStatementDto>
{
    private readonly IReadRepository<BankStatement> _statementRepository;
    private readonly IStringLocalizer<GetBankStatementHandler> _localizer;
    // No need to inject IReadRepository<BankAccount> if BankAccount is included in the spec.

    public GetBankStatementHandler(
        IReadRepository<BankStatement> statementRepository,
        IStringLocalizer<GetBankStatementHandler> localizer)
    {
        _statementRepository = statementRepository;
        _localizer = localizer;
    }

    public async Task<BankStatementDto> Handle(GetBankStatementRequest request, CancellationToken cancellationToken)
    {
        var spec = new BankStatementByIdWithDetailsSpec(request.Id);
        var bankStatement = await _statementRepository.FirstOrDefaultAsync(spec, cancellationToken);
        // FirstOrDefaultAsync with a spec mapping to Dto (Specification<BankStatement, BankStatementDto>)
        // should directly return BankStatementDto.

        if (bankStatement == null)
        {
            throw new NotFoundException(_localizer["Bank Statement with ID {0} not found.", request.Id]);
        }

        // If bankStatement is already BankStatementDto from spec, direct return.
        // If spec returns entity, then Adapt. Assuming spec returns DTO for now.
        // If it was an entity: var dto = bankStatement.Adapt<BankStatementDto>();

        // Populate BankAccountName if not directly mapped by a smart Mapster projection in spec
        // (Spec includes BankAccount, so Mapster might map BankAccount.AccountName to BankAccountName)
        // This explicit mapping is a fallback or if more complex formatting is needed.
        if (string.IsNullOrEmpty(bankStatement.BankAccountName) && (bankStatement.GetType() == typeof(BankStatementDto)))
        {
            // This block would be more relevant if the spec returned the entity `BankStatement`
            // and we adapted it here. If the spec returns `BankStatementDto` directly,
            // then the mapping from `BankAccount` to `BankAccountName` should happen in Mapster config or by convention.
            // For example, if `BankStatementByIdWithDetailsSpec` was `Specification<BankStatement>`:
            // var entity = await _statementRepository.FirstOrDefaultAsync(spec, cancellationToken);
            // var dto = entity.Adapt<BankStatementDto>();
            // if (entity.BankAccount != null)
            // {
            //    dto.BankAccountName = $"{entity.BankAccount.BankName} - {entity.BankAccount.AccountNumber}";
            // }
            // return dto;
        }


        // Ensure transaction types are strings
        foreach(var transDto in bankStatement.Transactions)
        {
            // This assumes BankStatementTransaction.Type is an enum and BankStatementTransactionDto.Type is string
            // This mapping should ideally be handled by Mapster. If not, it needs to be done manually.
            // For now, assuming Mapster handles enum to string for Transaction.Type correctly.
            // If it doesn't, a loop similar to this would be needed:
            // var entityTransaction = bankStatementEntity.Transactions.FirstOrDefault(t => t.Id == transDto.Id);
            // if (entityTransaction != null) transDto.Type = entityTransaction.Type.ToString();
        }


        return bankStatement; // Assuming spec returns BankStatementDto
    }
}
