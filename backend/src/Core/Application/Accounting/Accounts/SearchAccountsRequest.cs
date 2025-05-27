using FSH.WebApi.Application.Common.Models;
using MediatR;

namespace FSH.WebApi.Application.Accounting.Accounts;

public class SearchAccountsRequest : PaginationFilter, IRequest<PaginationResponse<AccountDto>>
{
    public string? Keyword { get; set; }
    public string? AccountType { get; set; } // String to be parsed to AccountType enum later
}
