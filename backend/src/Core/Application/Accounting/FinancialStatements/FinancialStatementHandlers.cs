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
    private readonly IRepository<Account> _accountRepository;
    private readonly IStringLocalizer<GenerateBalanceSheetHandler> _localizer;
    private readonly ILogger<GenerateBalanceSheetHandler> _logger;
    private readonly IRepository<Transaction> _transactionRepository; // Needed to calculate balances as of date

    public GenerateBalanceSheetHandler(
        IRepository<Account> accountRepository,
        IStringLocalizer<GenerateBalanceSheetHandler> localizer,
        ILogger<GenerateBalanceSheetHandler> logger,
        IRepository<Transaction> transactionRepository)
    {
        _accountRepository = accountRepository;
        _localizer = localizer;
        _logger = logger;
        _transactionRepository = transactionRepository;
    }

    public async Task<BalanceSheetDto> Handle(GenerateBalanceSheetRequest request, CancellationToken cancellationToken)
    {
        var allAccounts = await _accountRepository.ListAsync(cancellationToken); // Get all accounts

        var statement = new BalanceSheetDto { AsOfDate = request.AsOfDate };

        foreach (var acc in allAccounts)
        {
            if (!acc.IsActive && acc.Balance == 0) continue; // Skip inactive zero-balance accounts

            // Calculate balance as of request.AsOfDate
            var transactions = await _transactionRepository.ListAsync(
                new TransactionsForAccountUpToDateSpec(acc.Id, request.AsOfDate), cancellationToken);

            decimal currentBalance = 0; // Start with 0, or account.InitialBalance if that was a concept
            foreach (var trans in transactions)
            {
                currentBalance += CalculateBalanceChange(trans.TransactionType, trans.Amount, acc.AccountType);
            }
            // The acc.Balance from repository is the *current* live balance, not necessarily as of AsOfDate.
            // So, we must calculate it.

            if (currentBalance == 0 && !acc.IsActive) continue; // Further filter out accounts that zero out by AsOfDate

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
                    // Retained Earnings is typically part of Equity.
                    // For a full Balance Sheet, net profit/loss from P&L for the period ending AsOfDate
                    // would be incorporated into Retained Earnings (an Equity account).
                    // Here, we're taking the balance of existing equity accounts.
                    statement.Equity.Add(line);
                    statement.TotalEquity += currentBalance;
                    break;
            }
        }

        statement.TotalLiabilitiesAndEquity = statement.TotalLiabilities + statement.TotalEquity;

        if (Math.Abs(statement.TotalAssets - statement.TotalLiabilitiesAndEquity) > 0.001m) // Using a small tolerance for decimal comparison
        {
            _logger.LogWarning(_localizer["Balance Sheet out of balance. Assets: {0}, Liabilities + Equity: {1}"], statement.TotalAssets, statement.TotalLiabilitiesAndEquity);
            // This indicates an issue, possibly with how transactions are recorded or balances calculated.
        }

        _logger.LogInformation(_localizer["Balance Sheet generated as of {0}"], request.AsOfDate);
        return statement;
    }
    private decimal CalculateBalanceChange(TransactionType transactionType, decimal amount, AccountType accountType)
    {
        bool isDebit = transactionType == TransactionType.Debit;
        switch (accountType)
        {
            case AccountType.Asset:
            case AccountType.Expense: // Expenses reduce equity, but here we look at their natural balance impact
                return isDebit ? amount : -amount;
            case AccountType.Liability:
            case AccountType.Equity:
            case AccountType.Revenue: // Revenue increases equity
                return isDebit ? -amount : amount;
            default:
                return 0;
        }
    }
}

// Specifications
public class AccountsByTypeSpec : Specification<Account>
{
    public AccountsByTypeSpec(AccountType accountType) =>
        Query.Where(a => a.AccountType == accountType && a.IsActive);
}

// Re-using from LedgerService or define locally if not shared.
// For this context, assuming it might be specific or slightly different.
public class TransactionsForAccountInPeriodSpec : Specification<Transaction>
{
    public TransactionsForAccountInPeriodSpec(Guid accountId, DateTime fromDate, DateTime toDate)
    {
        Query
            .Where(t => t.AccountId == accountId && t.JournalEntry.IsPosted && t.JournalEntry.PostedDate >= fromDate && t.JournalEntry.PostedDate < toDate.AddDays(1))
            .Include(t => t.JournalEntry)
            .OrderBy(t => t.JournalEntry.PostedDate);
    }
}

public class TransactionsForAccountUpToDateSpec : Specification<Transaction>
{
    public TransactionsForAccountUpToDateSpec(Guid accountId, DateTime toDate)
    {
        Query
            .Where(t => t.AccountId == accountId && t.JournalEntry.IsPosted && t.JournalEntry.PostedDate < toDate.AddDays(1)) // up to and including 'toDate'
            .Include(t => t.JournalEntry)
            .OrderBy(t => t.JournalEntry.PostedDate);
    }
}

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
