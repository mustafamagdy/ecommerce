using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For BankAccount
using System;

namespace FSH.WebApi.Application.Accounting.BankAccounts.Specifications;

// This specification is primarily for fetching the BankAccount entity.
// The linked GL Account (Account entity) details are typically fetched separately in handlers
// using the GLAccountId from the BankAccount.
public class BankAccountWithGLByIdSpec : Specification<BankAccount>, ISingleResultSpecification
{
    public BankAccountWithGLByIdSpec(Guid bankAccountId)
    {
        Query.Where(ba => ba.Id == bankAccountId);
        // If GLAccount were a navigation property on BankAccount, you would add:
        // Query.Include(ba => ba.GLAccount);
    }
}
