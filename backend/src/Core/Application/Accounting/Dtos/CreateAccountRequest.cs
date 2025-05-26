using FSH.WebApi.Domain.Accounting.Enums;

namespace FSH.WebApi.Application.Accounting.Dtos;

public class CreateAccountRequest
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; }
}
