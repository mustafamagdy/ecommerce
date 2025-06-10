using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting; // For BankStatement, BankAccount
using MediatR;
using Microsoft.Extensions.Localization; // If needed for logging/messages
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.BankStatements;

public class SearchBankStatementsHandler : IRequestHandler<SearchBankStatementsRequest, PaginationResponse<BankStatementDto>>
{
    private readonly IReadRepository<BankStatement> _statementRepository;
    // No need to inject IReadRepository<BankAccount> if BankAccount is included in the spec for name population

    public SearchBankStatementsHandler(IReadRepository<BankStatement> statementRepository)
    {
        _statementRepository = statementRepository;
    }

    public async Task<PaginationResponse<BankStatementDto>> Handle(SearchBankStatementsRequest request, CancellationToken cancellationToken)
    {
        var spec = new BankStatementsBySearchFilterSpec(request); // This spec includes BankAccount
        // Assuming spec is Specification<BankStatement, BankStatementDto> and returns DTOs

        var statements = await _statementRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _statementRepository.CountAsync(spec, cancellationToken);

        // If statements is List<BankStatementDto> from the spec, and BankAccountName is not populated by Mapster projection.
        // This explicit mapping is a fallback.
        if (statements.Any() && statements.First().GetType() == typeof(BankStatementDto) && string.IsNullOrEmpty(statements.First().BankAccountName))
        {
            // This scenario is less likely if Mapster projection from Entity to DTO is well-defined.
            // It implies the spec returned DTOs but without full BankAccountName.
            // A more common pattern if spec returns entities:
            // var statementEntities = await _statementRepository.ListAsync(entitySpec, cancellationToken);
            // var dtos = statementEntities.Adapt<List<BankStatementDto>>();
            // foreach(var dto in dtos) { /* populate BankAccountName from corresponding entity */ }
            // For now, assuming the spec returns DTOs and Mapster handles BankAccount.AccountName -> BankAccountName by convention if BankAccount is included.
        }

        // If transactions were part of the list DTO and needed their types converted (unlikely for a list view DTO)
        // foreach(var stmtDto in statements) { /* similar loop as in GetBankStatementHandler for transactions */ }

        return new PaginationResponse<BankStatementDto>(statements, totalCount, request.PageNumber, request.PageSize);
    }
}
