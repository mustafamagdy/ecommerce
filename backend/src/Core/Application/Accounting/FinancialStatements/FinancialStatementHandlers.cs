using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Mapster;
using FSH.WebApi.Application.Common.Exporters;
using FSH.WebApi.Application.Printing;
using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Accounting.FinancialStatements;

public class GenerateProfitAndLossHandler : IRequestHandler<GenerateProfitAndLossRequest, ProfitAndLossStatementDto>
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IStringLocalizer<GenerateProfitAndLossHandler> _localizer;
    private readonly ILogger<GenerateProfitAndLossHandler> _logger;

    public GenerateProfitAndLossHandler(
        IRepository<Account> accountRepository,
        IRepository<Transaction> transactionRepository,
        IStringLocalizer<GenerateProfitAndLossHandler> localizer,
        ILogger<GenerateProfitAndLossHandler> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<ProfitAndLossStatementDto> Handle(GenerateProfitAndLossRequest request, CancellationToken cancellationToken)
    {
        var revenueAccounts = await _accountRepository.ListAsync(new AccountsByTypeSpec(AccountType.Revenue), cancellationToken);
        var expenseAccounts = await _accountRepository.ListAsync(new AccountsByTypeSpec(AccountType.Expense), cancellationToken);

        var statement = new ProfitAndLossStatementDto
        {
            FromDate = request.FromDate,
            ToDate = request.ToDate
        };

        // Calculate Revenue
        foreach (var acc in revenueAccounts)
        {
            var transactions = await _transactionRepository.ListAsync(
                new TransactionsForAccountInPeriodSpec(acc.Id, request.FromDate, request.ToDate), cancellationToken);
            decimal accountTotal = transactions.Sum(t => t.TransactionType == TransactionType.Credit ? t.Amount : -t.Amount); // Revenue typically Credit balance

            if (accountTotal != 0) // Only include accounts with activity in the period
            {
                statement.Revenue.Add(new FinancialStatementLineDto { AccountName = acc.AccountName, AccountNumber = acc.AccountNumber, Amount = accountTotal });
                statement.TotalRevenue += accountTotal;
            }
        }

        // Calculate Expenses (Operating Expenses for this example, COGS could be a specific subset)
        foreach (var acc in expenseAccounts)
        {
            var transactions = await _transactionRepository.ListAsync(
                new TransactionsForAccountInPeriodSpec(acc.Id, request.FromDate, request.ToDate), cancellationToken);
            decimal accountTotal = transactions.Sum(t => t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount); // Expenses typically Debit balance

            if (accountTotal != 0) // Only include accounts with activity
            {
                // Simple assumption: all expenses are operating expenses.
                // A more complex chart of accounts might differentiate COGS.
                statement.OperatingExpenses.Add(new FinancialStatementLineDto { AccountName = acc.AccountName, AccountNumber = acc.AccountNumber, Amount = accountTotal });
                statement.TotalOperatingExpenses += accountTotal;
            }
        }
        // Assuming COGS is part of Operating Expenses for now, or not explicitly tracked as a separate high-level category in this basic setup.
        // If COGS were separate, you'd filter specific expense accounts tagged as COGS.
        statement.TotalCostOfGoodsSold = 0; // Explicitly set if not calculated

        statement.GrossProfit = statement.TotalRevenue - statement.TotalCostOfGoodsSold;
        statement.NetProfit = statement.GrossProfit - statement.TotalOperatingExpenses;

        _logger.LogInformation(_localizer["Profit and Loss Statement generated from {0} to {1}"], request.FromDate, request.ToDate);
        return statement;
    }
}

public class GenerateBalanceSheetHandler : IRequestHandler<GenerateBalanceSheetRequest, BalanceSheetDto>
{
    private readonly IReadRepository<Account> _accountRepository; // Changed to IReadRepository
    private readonly IStringLocalizer<GenerateBalanceSheetHandler> _localizer;
    private readonly ILogger<GenerateBalanceSheetHandler> _logger;
    private readonly IReadRepository<Transaction> _transactionRepository; // Changed to IReadRepository

    public GenerateBalanceSheetHandler(
        IReadRepository<Account> accountRepository, // Changed to IReadRepository
        IStringLocalizer<GenerateBalanceSheetHandler> localizer,
        ILogger<GenerateBalanceSheetHandler> logger,
        IReadRepository<Transaction> transactionRepository) // Changed to IReadRepository
    {
        _accountRepository = accountRepository;
        _localizer = localizer;
        _logger = logger;
        _transactionRepository = transactionRepository;
    }

    public async Task<BalanceSheetDto> Handle(GenerateBalanceSheetRequest request, CancellationToken cancellationToken)
    {
        // Define P&L period for current earnings calculation
        // Simplification: Current calendar year up to AsOfDate
        var profitAndLossStartDate = new DateTime(request.AsOfDate.Year, 1, 1);
        if (profitAndLossStartDate > request.AsOfDate) // Handle cases where AsOfDate is early in the year
        {
            // This might mean taking from previous year's start, or just a very short period.
            // For simplicity, if AsOfDate is Jan 1st, P&L period is just that day.
            // Or adjust to previous year if that's the fiscal convention.
            // For now, if AsOfDate is e.g. Jan 1 2023, P&L period is Jan 1 2023 to Jan 1 2023.
            // If fiscal year starts differently, this logic would need adjustment.
             profitAndLossStartDate = request.AsOfDate.Month == 1 && request.AsOfDate.Day == 1 ? request.AsOfDate : new DateTime(request.AsOfDate.Year, 1, 1);
        }


        var allAccounts = await _accountRepository.ListAsync(new AccountsByTypeSpec(null, true), cancellationToken); // Get all active accounts

        var statement = new BalanceSheetDto { AsOfDate = request.AsOfDate };

        // Calculate Current Period Net Profit/Loss
        decimal currentNetProfit = 0m;
        decimal totalPeriodRevenue = 0m;
        decimal totalPeriodExpenses = 0m;

        var revenueAccounts = allAccounts.Where(a => a.AccountType == AccountType.Revenue).ToList();
        var expenseAccounts = allAccounts.Where(a => a.AccountType == AccountType.Expense).ToList();

        foreach (var acc in revenueAccounts)
        {
            var transactions = await _transactionRepository.ListAsync(
                new TransactionsForAccountInPeriodSpec(acc.Id, profitAndLossStartDate, request.AsOfDate), cancellationToken);
            // Revenue: Credit increases balance, Debit decreases.
            totalPeriodRevenue += transactions.Sum(t => t.TransactionType == TransactionType.Credit ? t.Amount : -t.Amount);
        }

        foreach (var acc in expenseAccounts)
        {
            var transactions = await _transactionRepository.ListAsync(
                new TransactionsForAccountInPeriodSpec(acc.Id, profitAndLossStartDate, request.AsOfDate), cancellationToken);
            // Expense: Debit increases balance, Credit decreases.
            totalPeriodExpenses += transactions.Sum(t => t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount);
        }
        // Assuming COGS is part of expenses for this calculation as per P&L handler's current simplification.
        currentNetProfit = totalPeriodRevenue - totalPeriodExpenses;


        // Calculate balances for Asset, Liability, and existing Equity accounts
        foreach (var acc in allAccounts)
        {
            // We only need to process A, L, E accounts for the main BS structure. Revenue/Expense already processed for NetProfit.
            if (acc.AccountType != AccountType.Asset && acc.AccountType != AccountType.Liability && acc.AccountType != AccountType.Equity)
            {
                continue;
            }

            // Calculate balance as of request.AsOfDate
            // This includes all transactions up to AsOfDate, effectively giving the closing balance for A, L, E accounts.
            var transactions = await _transactionRepository.ListAsync(
                new TransactionsForAccountUpToDateSpec(acc.Id, request.AsOfDate), cancellationToken);

            decimal accountBalanceAsOfDate = 0;
            foreach (var trans in transactions)
            {
                accountBalanceAsOfDate += CalculateBalanceChange(trans.TransactionType, trans.Amount, acc.AccountType);
            }

            // Only include if there's a balance or if it's an active account (even with zero balance)
            if (accountBalanceAsOfDate == 0 && !acc.IsActive) continue;

            var line = new FinancialStatementLineDto { AccountName = acc.AccountName, AccountNumber = acc.AccountNumber, Amount = accountBalanceAsOfDate };

            switch (acc.AccountType)
            {
                case AccountType.Asset:
                    statement.Assets.Add(line);
                    statement.TotalAssets += accountBalanceAsOfDate;
                    break;
                case AccountType.Liability:
                    statement.Liabilities.Add(line);
                    // Liabilities have credit normal balance. If CalculateBalanceChange returns positive for credit normal, this is correct.
                    // CalculateBalanceChange for Liability: Credit is positive, Debit is negative.
                    // So, if currentBalance is positive, it's a credit balance (normal for liability).
                    statement.TotalLiabilities += accountBalanceAsOfDate;
                    break;
                case AccountType.Equity:
                    statement.Equity.Add(line);
                    // Equity has credit normal balance.
                    statement.TotalEquity += accountBalanceAsOfDate;
                    break;
            }
        }

        // Add Current Period Earnings to Equity
        if (currentNetProfit != 0) // Only add if there's a profit or loss
        {
            statement.Equity.Add(new FinancialStatementLineDto
            {
                AccountName = "Current Period Earnings", // Or "Retained Earnings - Current Period"
                AccountNumber = "CPE", // Placeholder account number
                Amount = currentNetProfit // Net profit increases equity (credit balance)
            });
        }
        statement.TotalEquity += currentNetProfit; // Add current net profit to total equity

        statement.TotalLiabilitiesAndEquity = statement.TotalLiabilities + statement.TotalEquity;

        // Final check for balance (Assets = Liabilities + Equity)
        if (Math.Abs(statement.TotalAssets - statement.TotalLiabilitiesAndEquity) > 0.01m) // Increased tolerance slightly for multi-step calcs
        {
            _logger.LogWarning(_localizer["Balance Sheet potentially out of balance. Assets: {0}, Liabilities + Equity: {1}. Difference: {2}"],
                statement.TotalAssets, statement.TotalLiabilitiesAndEquity, statement.TotalAssets - statement.TotalLiabilitiesAndEquity);
        }

        _logger.LogInformation(_localizer["Balance Sheet generated as of {0}"], request.AsOfDate);
        return statement;
    }

    // CalculateBalanceChange helper is defined below, and assumed to be correct.
    // It needs to correctly reflect how debits/credits affect the balance of different account types.
    // Asset: Debit increases, Credit decreases
    // Liability: Debit decreases, Credit increases
    // Equity: Debit decreases, Credit increases
    // Revenue: Debit decreases, Credit increases (Revenue is like negative expense/contra-expense from balance perspective)
    // Expense: Debit increases, Credit decreases (Expense is like negative revenue/contra-revenue from balance perspective)
    // The existing CalculateBalanceChange seems to handle Asset, Liability, Equity correctly for their normal balances.
    // For Revenue: Credit is income (+), Debit is reduction (-). Handler returns: isDebit ? -amount : amount. Correct.
    // For Expense: Debit is expense (+), Credit is reduction (-). Handler returns: isDebit ? amount : -amount. Correct.
    private decimal CalculateBalanceChange(TransactionType transactionType, decimal amount, AccountType accountType)
    {
        bool isDebit = transactionType == TransactionType.Debit;
        switch (accountType)
        {
            case AccountType.Asset:
            case AccountType.Expense:
                return isDebit ? amount : -amount;
            case AccountType.Liability:
            case AccountType.Equity:
            case AccountType.Revenue:
                return isDebit ? -amount : amount;
            default:
                return 0;
        }
    }
}


// Modified AccountsByTypeSpec to accept nullable AccountType and IsActive flag
public class AccountsByTypeSpec : Specification<Account>
{
    public AccountsByTypeSpec(AccountType? accountType, bool onlyActive = true)
    {
        if (accountType.HasValue)
        {
            Query.Where(a => a.AccountType == accountType.Value);
        }
        if (onlyActive)
        {
            Query.Where(a => a.IsActive);
        }
    }
}


// TransactionsForAccountInPeriodSpec: Assumed to be correct and fetches transactions for a specific account within a period.
// It should filter by JournalEntry.IsPosted and use JournalEntry.PostedDate.
// Existing definition:
// public class TransactionsForAccountInPeriodSpec : Specification<Transaction>
// {
//     public TransactionsForAccountInPeriodSpec(Guid accountId, DateTime fromDate, DateTime toDate)
//     {
//         Query
//             .Where(t => t.AccountId == accountId && t.JournalEntry.IsPosted && t.JournalEntry.PostedDate >= fromDate && t.JournalEntry.PostedDate < toDate.AddDays(1))
//             .Include(t => t.JournalEntry)
//             .OrderBy(t => t.JournalEntry.PostedDate);
//     }
// }


// TransactionsForAccountUpToDateSpec: Assumed to be correct and fetches transactions for an account up to a certain date.
// Existing definition:
// public class TransactionsForAccountUpToDateSpec : Specification<Transaction>
// {
//     public TransactionsForAccountUpToDateSpec(Guid accountId, DateTime toDate)
//     {
//         Query
//             .Where(t => t.AccountId == accountId && t.JournalEntry.IsPosted && t.JournalEntry.PostedDate < toDate.AddDays(1)) // up to and including 'toDate'
//             .Include(t => t.JournalEntry)
//             .OrderBy(t => t.JournalEntry.PostedDate);
//     }
// }


public class GenerateProfitAndLossReportHandler : IRequestHandler<GenerateProfitAndLossReportRequest, Stream>
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IStringLocalizer<GenerateProfitAndLossReportHandler> _localizer;
    private readonly ILogger<GenerateProfitAndLossReportHandler> _logger;
    private readonly IReadRepository<ProfitAndLossReport> _templateRepository;
    private readonly IPdfWriter _pdfWriter;
    private readonly ISubscriptionTypeResolver _subscriptionTypeResolver;

    public GenerateProfitAndLossReportHandler(
        IRepository<Account> accountRepository,
        IRepository<Transaction> transactionRepository,
        IReadRepository<ProfitAndLossReport> templateRepository,
        IPdfWriter pdfWriter,
        ISubscriptionTypeResolver subscriptionTypeResolver,
        IStringLocalizer<GenerateProfitAndLossReportHandler> localizer,
        ILogger<GenerateProfitAndLossReportHandler> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _templateRepository = templateRepository;
        _pdfWriter = pdfWriter;
        _subscriptionTypeResolver = subscriptionTypeResolver;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Stream> Handle(GenerateProfitAndLossReportRequest request, CancellationToken cancellationToken)
    {
        var revenueAccounts = await _accountRepository.ListAsync(new AccountsByTypeSpec(AccountType.Revenue), cancellationToken);
        var expenseAccounts = await _accountRepository.ListAsync(new AccountsByTypeSpec(AccountType.Expense), cancellationToken);

        var statement = new ProfitAndLossStatementDto
        {
            FromDate = request.FromDate,
            ToDate = request.ToDate
        };

        foreach (var acc in revenueAccounts)
        {
            var transactions = await _transactionRepository.ListAsync(
                new TransactionsForAccountInPeriodSpec(acc.Id, request.FromDate, request.ToDate), cancellationToken);
            decimal accountTotal = transactions.Sum(t => t.TransactionType == TransactionType.Credit ? t.Amount : -t.Amount);
            if (accountTotal != 0)
            {
                statement.Revenue.Add(new FinancialStatementLineDto { AccountName = acc.AccountName, AccountNumber = acc.AccountNumber, Amount = accountTotal });
                statement.TotalRevenue += accountTotal;
            }
        }

        foreach (var acc in expenseAccounts)
        {
            var transactions = await _transactionRepository.ListAsync(
                new TransactionsForAccountInPeriodSpec(acc.Id, request.FromDate, request.ToDate), cancellationToken);
            decimal accountTotal = transactions.Sum(t => t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount);
            if (accountTotal != 0)
            {
                statement.OperatingExpenses.Add(new FinancialStatementLineDto { AccountName = acc.AccountName, AccountNumber = acc.AccountNumber, Amount = accountTotal });
                statement.TotalOperatingExpenses += accountTotal;
            }
        }

        statement.TotalCostOfGoodsSold = 0;
        statement.GrossProfit = statement.TotalRevenue - statement.TotalCostOfGoodsSold;
        statement.NetProfit = statement.GrossProfit - statement.TotalOperatingExpenses;

        var template = await _templateRepository.FirstOrDefaultAsync(
            new SingleResultSpecification<ProfitAndLossReport>().Query.Include(a => a.Sections.OrderBy(x => x.Order)).Where(a => a.Active).Specification,
            cancellationToken);

        var bound = new BoundTemplate(template);
        bound.BindTemplate(statement);

        var subscription = _subscriptionTypeResolver.Resolve();
        var doc = new InvoiceDocument(subscription, bound);
        return _pdfWriter.WriteToStream(doc);
    }

}

public class GenerateBalanceSheetReportHandler : IRequestHandler<GenerateBalanceSheetReportRequest, Stream>
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IStringLocalizer<GenerateBalanceSheetReportHandler> _localizer;
    private readonly ILogger<GenerateBalanceSheetReportHandler> _logger;
    private readonly IReadRepository<BalanceSheetReport> _templateRepository;
    private readonly IPdfWriter _pdfWriter;
    private readonly ISubscriptionTypeResolver _subscriptionTypeResolver;

    public GenerateBalanceSheetReportHandler(
        IRepository<Account> accountRepository,
        IRepository<Transaction> transactionRepository,
        IReadRepository<BalanceSheetReport> templateRepository,
        IPdfWriter pdfWriter,
        ISubscriptionTypeResolver subscriptionTypeResolver,
        IStringLocalizer<GenerateBalanceSheetReportHandler> localizer,
        ILogger<GenerateBalanceSheetReportHandler> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _templateRepository = templateRepository;
        _pdfWriter = pdfWriter;
        _subscriptionTypeResolver = subscriptionTypeResolver;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Stream> Handle(GenerateBalanceSheetReportRequest request, CancellationToken cancellationToken)
    {
        var allAccounts = await _accountRepository.ListAsync(cancellationToken);

        var statement = new BalanceSheetDto { AsOfDate = request.AsOfDate };

        foreach (var acc in allAccounts)
        {
            if (!acc.IsActive && acc.Balance == 0) continue;

            var transactions = await _transactionRepository.ListAsync(
                new TransactionsForAccountUpToDateSpec(acc.Id, request.AsOfDate), cancellationToken);

            decimal currentBalance = 0;
            foreach (var trans in transactions)
            {
                currentBalance += CalculateBalanceChange(trans.TransactionType, trans.Amount, acc.AccountType);
            }

            if (currentBalance == 0 && !acc.IsActive) continue;

            var line = new FinancialStatementLineDto { AccountName = acc.AccountName, AccountNumber = acc.AccountNumber, Amount = currentBalance };

            switch (acc.AccountType)
            {
                case AccountType.Asset:
                    statement.Assets.Add(line);
                    statement.TotalAssets += currentBalance;
                    break;
                case AccountType.Liability:
                    statement.Liabilities.Add(line);
                    statement.TotalLiabilities += currentBalance;
                    break;
                case AccountType.Equity:
                    statement.Equity.Add(line);
                    statement.TotalEquity += currentBalance;
                    break;
            }
        }

        statement.TotalLiabilitiesAndEquity = statement.TotalLiabilities + statement.TotalEquity;

        var template = await _templateRepository.FirstOrDefaultAsync(
            new SingleResultSpecification<BalanceSheetReport>().Query.Include(a => a.Sections.OrderBy(x => x.Order)).Where(a => a.Active).Specification,
            cancellationToken);

        var bound = new BoundTemplate(template);
        bound.BindTemplate(statement);

        var subscription = _subscriptionTypeResolver.Resolve();
        var doc = new InvoiceDocument(subscription, bound);
        return _pdfWriter.WriteToStream(doc);
    }

    private decimal CalculateBalanceChange(TransactionType transactionType, decimal amount, AccountType accountType)
    {
        bool isDebit = transactionType == TransactionType.Debit;
        return accountType switch
        {
            AccountType.Asset or AccountType.Expense => isDebit ? amount : -amount,
            AccountType.Liability or AccountType.Equity or AccountType.Revenue => isDebit ? -amount : amount,
            _ => 0,
        };
    }
}
