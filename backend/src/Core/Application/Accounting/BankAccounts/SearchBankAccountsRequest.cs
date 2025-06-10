using FSH.WebApi.Application.Common.Models;
using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

public class SearchBankAccountsRequest : PaginationFilter, IRequest<PaginationResponse<BankAccountDto>>
{
    public string? NameKeyword { get; set; } // Search in AccountName, BankName, AccountNumber
    public string? Currency { get; set; }
    public Guid? GLAccountId { get; set; }
    public bool? IsActive { get; set; }
}
