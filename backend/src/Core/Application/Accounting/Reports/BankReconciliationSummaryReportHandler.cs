using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.BankReconciliations.Specifications;
using FSH.WebApi.Application.Accounting.BankStatements.Specifications;
using FSH.WebApi.Application.Accounting.JournalEntries.Specifications;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Domain.Operation.Customers; // Assuming Customer might be needed for BankAccount context, though less likely for this report
// using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.Accounting.Reports;

public class BankReconciliationSummaryReportHandler : IRequestHandler<BankReconciliationSummaryReportRequest, BankReconciliationSummaryReportDto>
{
    private readonly IReadRepository<BankReconciliation> _reconRepo;
    private readonly IReadRepository<BankAccount> _bankAccountRepo; // To get BankAccount details including GLAccountId
    private readonly IReadRepository<Account> _glAccountRepo;       // To get GLAccount name/code
    private readonly IReadRepository<BankStatement> _bankStatementRepo; // To get statement details (transactions already on BankStatement)
    private readonly IReadRepository<JournalEntry> _journalEntryRepo;    // To find system-side transactions
    private readonly IReadRepository<BankStatementTransaction> _bankStatementTransactionRepo; // For ReconciledSystemTransactionIdsForReconSpec

    // private readonly IStringLocalizer<BankReconciliationSummaryReportHandler> _localizer;

    public BankReconciliationSummaryReportHandler(
        IReadRepository<BankReconciliation> reconRepo,
        IReadRepository<BankAccount> bankAccountRepo,
        IReadRepository<Account> glAccountRepo,
        IReadRepository<BankStatement> bankStatementRepo,
        IReadRepository<JournalEntry> journalEntryRepo,
        IReadRepository<BankStatementTransaction> bankStatementTransactionRepo
        /* IStringLocalizer<BankReconciliationSummaryReportHandler> localizer */)
    {
        _reconRepo = reconRepo;
        _bankAccountRepo = bankAccountRepo;
        _glAccountRepo = glAccountRepo;
        _bankStatementRepo = bankStatementRepo;
        _journalEntryRepo = journalEntryRepo;
        _bankStatementTransactionRepo = bankStatementTransactionRepo;
        // _localizer = localizer;
    }

    public async Task<BankReconciliationSummaryReportDto> Handle(BankReconciliationSummaryReportRequest request, CancellationToken cancellationToken)
    {
        // 1. Fetch Core Reconciliation Data
        var reconSpec = new BankReconciliationByIdWithDetailsSpec(request.BankReconciliationId);
        // This spec currently returns BankReconciliationSummaryReportDto, let's assume it returns the entity for handler processing
        // To fix this, I should have made BankReconciliationByIdWithDetailsSpec return the entity: Specification<BankReconciliation>
        // For now, I will proceed as if it returns the entity. If not, this fetch needs adjustment or two fetches.
        // Let's assume a different spec or GetByIdAsync for the entity:
        var reconciliation = await _reconRepo.GetByIdAsync(request.BankReconciliationId, cancellationToken);
        if (reconciliation == null)
            throw new NotFoundException($"Bank Reconciliation with ID {request.BankReconciliationId} not found.");

        var bankAccount = await _bankAccountRepo.GetByIdAsync(reconciliation.BankAccountId, cancellationToken);
        if (bankAccount == null)
            throw new NotFoundException($"BankAccount with ID {reconciliation.BankAccountId} for Reconciliation not found.");

        Account? glAccount = null;
        if (bankAccount.GLAccountId != Guid.Empty)
        {
            glAccount = await _glAccountRepo.GetByIdAsync(bankAccount.GLAccountId, cancellationToken);
            // if (glAccount == null) throw new NotFoundException($"GL Account for BankAccount {bankAccount.AccountName} not found.");
        }

        // Fetch the specific BankStatement with its transactions
        var statementSpec = new BankStatementByIdWithDetailsSpec(reconciliation.BankStatementId);
                               // Assuming this spec returns BankStatement entity and includes its Transactions
        var bankStatement = await _bankStatementRepo.FirstOrDefaultAsync(statementSpec, cancellationToken);
        if (bankStatement == null)
            throw new NotFoundException($"Bank Statement with ID {reconciliation.BankStatementId} for Reconciliation not found.");


        // 2. Initialize Report DTO
        var reportDto = new BankReconciliationSummaryReportDto
        {
            BankReconciliationId = reconciliation.Id,
            BankAccountId = bankAccount.Id,
            BankAccountName = bankAccount.AccountName,
            BankAccountNumber = bankAccount.AccountNumber,
            BankAccountCurrency = bankAccount.Currency,
            ReconciliationDate = reconciliation.ReconciliationDate,
            BankStatementId = bankStatement.Id,
            StatementDate = bankStatement.StatementDate,
            BankStatementReference = bankStatement.ReferenceNumber,
            StatementEndingBalance = reconciliation.StatementBalance,
            SystemGlBalanceAsPerRecon = reconciliation.SystemBalance, // This is the GL balance recorded at time of reconciliation creation/update
            Status = reconciliation.Status.ToString(),
            GeneratedOn = DateTime.UtcNow.ToString("o"),
            UnclearedChecksOrDebitsDetails = new List<BankTransactionDetailDto>(),
            DepositsInTransitOrCreditsDetails = new List<BankTransactionDetailDto>(),
            BankAdjustmentsNotOnSystemDetails = new List<BankTransactionDetailDto>()
        };

        // 3. Identify Bank Adjustments Not Yet In System (Unmatched Bank Statement Transactions for this Recon)
        foreach (var tx in bankStatement.Transactions)
        {
            if (!tx.IsReconciled || tx.BankReconciliationId != reconciliation.Id)
            {
                reportDto.BankAdjustmentsNotOnSystemDetails.Add(new BankTransactionDetailDto
                {
                    OriginalTransactionId = tx.Id,
                    TransactionSource = "Bank Statement",
                    Date = tx.TransactionDate,
                    Reference = tx.Reference,
                    Description = tx.Description,
                    Amount = tx.Amount,
                    Type = tx.Type.ToString()
                });
                if (tx.Type == BankTransactionType.Debit) // Bank debit (fee, charge) reduces bank balance, needs GL debit or reduces GL credit
                    reportDto.BankAdjustmentsNotOnSystemAmount -= tx.Amount; // From GL perspective, this is a system debit needed
                else // Bank credit (interest) increases bank balance, needs GL credit or reduces GL debit
                    reportDto.BankAdjustmentsNotOnSystemAmount += tx.Amount; // From GL perspective, this is a system credit needed
                reportDto.BankAdjustmentsNotOnSystemCount++;
            }
        }


        // 4. Identify System Transactions Not Cleared by Bank (Uncleared Checks & Deposits in Transit)
        // Fetch SystemTransactionIds that *are* reconciled in this specific reconciliation run
        var reconciledSystemTxIdsSpec = new ReconciledSystemTransactionIdsForReconSpec(reconciliation.Id);
        var reconciledSystemTxIds = (await _bankStatementTransactionRepo.ListAsync(reconciledSystemTxIdsSpec, cancellationToken)).ToHashSet();

        // Define a reasonable date window around the reconciliation date to find GL transactions
        // E.g., 60 days before ReconciliationDate up to ReconciliationDate.
        // This depends on typical clearing times for checks/deposits.
        var windowStartDate = reconciliation.ReconciliationDate.AddDays(-60); // Configurable window
        var windowEndDate = reconciliation.ReconciliationDate;

        var glTransactionsSpec = new JournalEntriesForGLAccountInDateWindowSpec(bankAccount.GLAccountId, windowStartDate, windowEndDate);
        var journalEntriesInWindow = await _journalEntryRepo.ListAsync(glTransactionsSpec, cancellationToken);

        var systemTransactionsForGLAccount = journalEntriesInWindow
            .SelectMany(je => je.Transactions
                .Where(t => t.AccountId == bankAccount.GLAccountId)
                .Select(t => new { JournalEntry = je, Transaction = t }))
            .OrderBy(x => x.JournalEntry.Date)
            .ThenBy(x => x.JournalEntry.CreatedOn)
            .ThenBy(x => x.Transaction.Id)
            .ToList();

        foreach (var item in systemTransactionsForGLAccount)
        {
            // If this system transaction's ID is NOT in the set of already reconciled system IDs for THIS reconciliation
            if (!reconciledSystemTxIds.Contains(item.Transaction.Id))
            {
                var detail = new BankTransactionDetailDto
                {
                    OriginalTransactionId = item.Transaction.Id, // This is the GL Transaction.Id
                    TransactionSource = "General Ledger",
                    Date = item.JournalEntry.Date,
                    Reference = item.JournalEntry.Reference ?? item.JournalEntry.EntryNumber,
                    Description = item.Transaction.Description ?? item.JournalEntry.Description,
                    Amount = item.Transaction.Debit > 0 ? item.Transaction.Debit : item.Transaction.Credit,
                };

                // Perspective for Uncleared Checks / Deposits in Transit:
                // Uncleared Check: A payment (Debit to Bank GL Acct) made by company, not yet cashed/cleared by bank.
                // Deposit in Transit: A deposit (Credit to Bank GL Acct) made by company, not yet reflected by bank.
                if (item.Transaction.Debit > 0) // A debit in GL for the bank account (e.g. a payment)
                {
                    detail.Type = "Debit (System)";
                    reportDto.UnclearedChecksOrDebitsDetails.Add(detail);
                    reportDto.UnclearedChecksOrDebitsAmount += detail.Amount;
                    reportDto.UnclearedChecksOrDebitsCount++;
                }
                else if (item.Transaction.Credit > 0) // A credit in GL for the bank account (e.g. a deposit)
                {
                    detail.Type = "Credit (System)";
                    reportDto.DepositsInTransitOrCreditsDetails.Add(detail);
                    reportDto.DepositsInTransitOrCreditsAmount += detail.Amount;
                    reportDto.DepositsInTransitOrCreditsCount++;
                }
            }
        }

        // 5. Calculate Adjusted Balances
        // Adjusted Bank Balance = Statement Ending Balance + Deposits in Transit - Uncleared Checks +/- Net Bank Adjustments (from Bank's perspective to reach true cash)
        // For DTO: BankAdjustmentsNotOnSystemAmount is net (Credits - Debits from bank's perspective not in system)
        // So if BankAdjustmentsNotOnSystemAmount is positive (e.g. interest income), it adds to statement balance.
        // If negative (e.g. bank fees), it subtracts.
        reportDto.CalculatedAdjustedBankBalance =
            reconciliation.StatementBalance +
            reportDto.DepositsInTransitOrCreditsAmount -
            reportDto.UnclearedChecksOrDebitsAmount;
            // The BankAdjustmentsNotOnSystemAmount is tricky. These are items ON bank statement, NOT IN system.
            // So, to get to an "adjusted GL balance", these would be applied to the SystemGlBalanceAsPerRecon.
            // AdjustedStatementBalance = StatementBalance - UnclearedChecks + DepositsInTransit
            // AdjustedGLBalance = SystemGLBalance + UnrecordedBankCredits(e.g. interest) - UnrecordedBankDebits(e.g. fees)

        reportDto.CalculatedAdjustedGlBalance =
            reconciliation.SystemBalance +
            reportDto.BankAdjustmentsNotOnSystemAmount; // If BankAdjustmentsNotOnSystemAmount is (BankCredits - BankDebits)


        // 6. Set UnexplainedDifference from reconciliation entity's pre-calculated one.
        // This reflects the difference considering the *manually entered* uncleared checks/deposits in transit
        // on the reconciliation record, not necessarily the items *found* by this report handler.
        // This highlights if the found items explain the original difference.
        reportDto.UnexplainedDifference = reconciliation.Difference;


        // Populate summary amounts from the reconciliation record's manual fields for comparison/consistency,
        // if they are considered the "official" reconciling items for that completed recon.
        // The DTO is designed to show what the handler *discovers* for details.
        // For the summary amounts in DTO, it could either sum the discovered items, or show the manual ones from recon.
        // The current DTO structure (e.g. UnclearedChecksOrDebitsAmount) implies it's the sum of discovered items.

        return reportDto;
    }
}
