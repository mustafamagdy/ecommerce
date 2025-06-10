using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For BankAccount
using System;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

// The "WithGL" part of the name implies including GL details.
// If GLAccount is a navigation property on BankAccount, it would be included here.
// Example: Query.Include(ba => ba.GLAccount);
// If not, the handler will fetch GL details separately using GLAccountId.
// For now, this spec is primarily for fetching the BankAccount by ID.
// The DTO mapping logic in the handler will be responsible for the "WithGL" aspect.
public class BankAccountByIdSpec : Specification<BankAccount, BankAccountDto>, ISingleResultSpecification
{
    public BankAccountByIdSpec(Guid bankAccountId)
    {
        Query
            .Where(ba => ba.Id == bankAccountId);
        // If GLAccount was a direct navigation property on BankAccount:
        // Query.Include(ba => ba.GLAccount);
    }
}
