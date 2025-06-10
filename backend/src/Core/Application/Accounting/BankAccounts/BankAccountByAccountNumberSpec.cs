using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

// Renamed to be more specific, as per common practice.
public class BankAccountByAccountNumberSpec : Specification<BankAccount>, ISingleResultSpecification
{
    public BankAccountByAccountNumberSpec(string accountNumber) =>
        Query.Where(ba => ba.AccountNumber == accountNumber);
}
