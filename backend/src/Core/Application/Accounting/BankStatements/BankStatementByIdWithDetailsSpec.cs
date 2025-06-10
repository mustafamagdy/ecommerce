using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For BankStatement, BankAccount, BankStatementTransaction
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.BankStatements;

public class BankStatementByIdWithDetailsSpec : Specification<BankStatement, BankStatementDto>, ISingleResultSpecification
{
    public BankStatementByIdWithDetailsSpec(Guid bankStatementId)
    {
        Query
            .Where(bs => bs.Id == bankStatementId)
            .Include(bs => bs.Transactions) // Include the transactions collection
            .Include(bs => bs.BankAccount);  // Include BankAccount to populate BankAccountName in DTO
                                            // If BankAccount itself has GLAccount nav prop, could extend here:
                                            // .ThenInclude(ba => ba.GLAccount)
    }
}
