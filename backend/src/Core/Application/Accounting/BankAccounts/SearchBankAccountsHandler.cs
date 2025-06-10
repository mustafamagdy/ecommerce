using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting; // For BankAccount, Account
using MediatR;
using Microsoft.Extensions.Localization; // For IStringLocalizer (if needed for logging/messages)
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

public class SearchBankAccountsHandler : IRequestHandler<SearchBankAccountsRequest, PaginationResponse<BankAccountDto>>
{
    private readonly IReadRepository<BankAccount> _bankAccountRepository;
    private readonly IReadRepository<Account> _glAccountRepository; // To fetch GL Account details

    public SearchBankAccountsHandler(
        IReadRepository<BankAccount> bankAccountRepository,
        IReadRepository<Account> glAccountRepository)
    {
        _bankAccountRepository = bankAccountRepository;
        _glAccountRepository = glAccountRepository;
    }

    public async Task<PaginationResponse<BankAccountDto>> Handle(SearchBankAccountsRequest request, CancellationToken cancellationToken)
    {
        var spec = new BankAccountsBySearchFilterSpec(request); // This spec returns BankAccountDto
        // If spec returns BankAccount (entity), then manual Adapt and GL population is needed.
        // Assuming spec is Specification<BankAccount, BankAccountDto> as per its definition.

        var bankAccounts = await _bankAccountRepository.ListAsync(spec, cancellationToken); // ListAsync should honor pagination from spec
        var totalCount = await _bankAccountRepository.CountAsync(spec, cancellationToken); // CountAsync with the same spec

        // If bankAccounts is List<BankAccountDto> and GLAccount fields are not populated by Spec's ProjectToType (e.g. via Mapster projection)
        // then we need to fetch GL Accounts for the current page of results.
        if (bankAccounts.Any() && string.IsNullOrEmpty(bankAccounts.First().GLAccountName)) // Check if GL details are missing
        {
            var glAccountIds = bankAccounts
                .Where(ba => ba.GLAccountId != Guid.Empty)
                .Select(ba => ba.GLAccountId)
                .Distinct()
                .ToList();

            if (glAccountIds.Any())
            {
                // Fetch all relevant GL Accounts in one go
                var glAccounts = (await _glAccountRepository.ListAsync(new GLAccountsByIdsSpec(glAccountIds), cancellationToken))
                                    .ToDictionary(acc => acc.Id);

                foreach (var baDto in bankAccounts)
                {
                    if (baDto.GLAccountId != Guid.Empty && glAccounts.TryGetValue(baDto.GLAccountId, out var glAccount))
                    {
                        baDto.GLAccountCode = glAccount.AccountNumber; // Assuming AccountNumber is Code
                        baDto.GLAccountName = glAccount.AccountName;
                    }
                }
            }
        }

        return new PaginationResponse<BankAccountDto>(bankAccounts, totalCount, request.PageNumber, request.PageSize);
    }
}

// Helper Spec to fetch multiple GL Accounts by IDs (if not already existing)
public class GLAccountsByIdsSpec : Specification<Account>
{
    public GLAccountsByIdsSpec(List<Guid> ids)
    {
        Query.Where(acc => ids.Contains(acc.Id));
    }
}
