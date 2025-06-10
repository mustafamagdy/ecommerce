using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For BankReconciliation, BankAccount, BankStatement
using System;

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class BankReconciliationByIdWithDetailsSpec : Specification<BankReconciliation, BankReconciliationDto>, ISingleResultSpecification
{
    public BankReconciliationByIdWithDetailsSpec(Guid bankReconciliationId)
    {
        Query
            .Where(br => br.Id == bankReconciliationId)
            .Include(br => br.BankAccount) // To populate BankAccountName in DTO
            .Include(br => br.BankStatement); // To populate BankStatementReference in DTO
                                            // Transactions are not directly part of BankReconciliation entity,
                                            // they will be fetched separately for the reconciliation UI.
    }
}
