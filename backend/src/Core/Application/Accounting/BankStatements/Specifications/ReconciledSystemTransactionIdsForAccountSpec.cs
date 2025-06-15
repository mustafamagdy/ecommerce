using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For BankStatementTransaction, BankStatement
using System;
using System.Collections.Generic; // Required for List
using System.Linq;

namespace FSH.WebApi.Application.Accounting.BankStatements.Specifications;

/// <summary>
/// Fetches a list of SystemTransactionIds (Guid) from BankStatementTransactions
/// that are marked as reconciled for a specific BankAccountId up to a given AsOfDate.
/// This helps identify which system-side (GL) transactions have already cleared the bank.
/// </summary>
public class ReconciledSystemTransactionIdsForAccountSpec : Specification<BankStatementTransaction, Guid>
{
    public ReconciledSystemTransactionIdsForAccountSpec(Guid bankAccountId, DateTime asOfDate)
    {
        Query
            .Include(bst => bst.BankStatement) // Need BankStatement to filter by BankAccountId
            .Where(bst => bst.BankStatement.BankAccountId == bankAccountId &&
                          bst.IsReconciled &&
                          bst.SystemTransactionId.HasValue &&
                          bst.TransactionDate <= asOfDate) // Consider only reconciliations up to the AsOfDate
            .Select(bst => bst.SystemTransactionId!.Value);
    }
}
