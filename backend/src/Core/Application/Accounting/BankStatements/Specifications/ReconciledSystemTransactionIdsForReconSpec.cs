using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For BankStatementTransaction
using System;
using System.Collections.Generic; // Required for List
using System.Linq;

namespace FSH.WebApi.Application.Accounting.BankStatements.Specifications;

/// <summary>
/// Fetches a list of SystemTransactionIds from BankStatementTransactions
/// that are marked as reconciled for a specific BankReconciliation.
/// </summary>
public class ReconciledSystemTransactionIdsForReconSpec : Specification<BankStatementTransaction, Guid> // Selects Guid
{
    public ReconciledSystemTransactionIdsForReconSpec(Guid bankReconciliationId)
    {
        Query
            .Where(bst => bst.BankReconciliationId == bankReconciliationId &&
                          bst.IsReconciled &&
                          bst.SystemTransactionId.HasValue)
            .Select(bst => bst.SystemTransactionId!.Value); // Select only the SystemTransactionId
    }
}
