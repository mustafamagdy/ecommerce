using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For BankReconciliation, BankAccount, BankStatement, Account
using System;
using System.Linq; // Required for ThenInclude

namespace FSH.WebApi.Application.Accounting.BankReconciliations.Specifications;

public class BankReconciliationByIdWithDetailsSpec : Specification<BankReconciliation, BankReconciliationSummaryReportDto>, ISingleResultSpecification
{
    public BankReconciliationByIdWithDetailsSpec(Guid bankReconciliationId)
    {
        Query
            .Where(br => br.Id == bankReconciliationId)
            .Include(br => br.BankAccount)
                // .ThenInclude(ba => ba.GLAccount) // Assuming GLAccount is a navigation property on BankAccount
                                                // If not, GLAccount details will be fetched separately in handler.
                                                // For now, let's assume it's NOT a direct nav prop from BankAccount for loose coupling.
                                                // The handler will fetch GLAccount separately using BankAccount.GLAccountId.
            .Include(br => br.BankStatement)
                .ThenInclude(bs => bs.Transactions); // Include all transactions for the linked statement
    }
}
