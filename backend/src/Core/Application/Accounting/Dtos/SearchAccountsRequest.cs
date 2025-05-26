using FSH.WebApi.Domain.Accounting.Enums;

namespace FSH.WebApi.Application.Accounting.Dtos;

public class SearchAccountsRequest
{
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public AccountType? AccountType { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
