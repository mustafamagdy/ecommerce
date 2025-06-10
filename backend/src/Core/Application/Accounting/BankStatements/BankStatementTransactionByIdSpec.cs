using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For BankStatementTransaction
using System;

namespace FSH.WebApi.Application.Accounting.BankStatements; // Placing in BankStatements folder as it queries BankStatementTransaction

public class BankStatementTransactionByIdSpec : Specification<BankStatementTransaction, BankStatementTransactionDto>, ISingleResultSpecification
{
    public BankStatementTransactionByIdSpec(Guid bankStatementTransactionId)
    {
        Query
            .Where(tx => tx.Id == bankStatementTransactionId);
        // No includes needed typically for just fetching one transaction by ID,
        // unless related entities like SystemTransaction (if it were a full nav prop) were needed.
    }
}
