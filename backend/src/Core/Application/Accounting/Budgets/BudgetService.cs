using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using FluentValidation;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Common.Contracts;
using Mapster;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Budgets;

// Using BudgetService as the class name to contain all handlers for Budgets.
public class CreateBudgetHandler : IRequestHandler<CreateBudgetRequest, Guid>
{
    private readonly IRepository<Budget> _budgetRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IStringLocalizer<CreateBudgetHandler> _localizer;
    private readonly ILogger<CreateBudgetHandler> _logger;

    public CreateBudgetHandler(
        IRepository<Budget> budgetRepository,
        IRepository<Account> accountRepository,
        IStringLocalizer<CreateBudgetHandler> localizer,
        ILogger<CreateBudgetHandler> logger)
    {
        _budgetRepository = budgetRepository;
        _accountRepository = accountRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
        {
            throw new NotFoundException(_localizer["Account with ID {0} not found.", request.AccountId]);
        }
        if(!account.IsActive)
        {
             throw new ValidationException(_localizer["Account with ID {0} is not active.", request.AccountId]);
        }


        var budget = new Budget(
            budgetName: request.BudgetName,
            accountId: request.AccountId,
            periodStartDate: request.PeriodStartDate,
            periodEndDate: request.PeriodEndDate,
            amount: request.Amount,
            description: request.Description
        );

        await _budgetRepository.AddAsync(budget, cancellationToken);

        _logger.LogInformation(_localizer["Budget created: {0}"], budget.Id);
        return budget.Id;
    }
}

public class UpdateBudgetHandler : IRequestHandler<UpdateBudgetRequest, Guid>
{
    private readonly IRepository<Budget> _budgetRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IStringLocalizer<UpdateBudgetHandler> _localizer;
    private readonly ILogger<UpdateBudgetHandler> _logger;

    public UpdateBudgetHandler(
        IRepository<Budget> budgetRepository,
        IRepository<Account> accountRepository,
        IStringLocalizer<UpdateBudgetHandler> localizer,
        ILogger<UpdateBudgetHandler> logger)
    {
        _budgetRepository = budgetRepository;
        _accountRepository = accountRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(request.Id, cancellationToken);
        if (budget == null)
        {
            throw new NotFoundException(_localizer["Budget with ID {0} not found.", request.Id]);
        }

        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
        {
            throw new NotFoundException(_localizer["Account with ID {0} not found.", request.AccountId]);
        }
         if(!account.IsActive)
        {
             throw new ValidationException(_localizer["Account with ID {0} is not active.", request.AccountId]);
        }

        budget.Update(
            budgetName: request.BudgetName,
            accountId: request.AccountId,
            periodStartDate: request.PeriodStartDate,
            periodEndDate: request.PeriodEndDate,
            amount: request.Amount,
            description: request.Description
        );

        await _budgetRepository.UpdateAsync(budget, cancellationToken);

        _logger.LogInformation(_localizer["Budget updated: {0}"], budget.Id);
        return budget.Id;
    }
}

public class GetBudgetHandler : IRequestHandler<GetBudgetRequest, BudgetDto>
{
    private readonly IRepository<Budget> _budgetRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IStringLocalizer<GetBudgetHandler> _localizer;

    public GetBudgetHandler(
        IRepository<Budget> budgetRepository,
        IRepository<Account> accountRepository,
        IRepository<Transaction> transactionRepository,
        IStringLocalizer<GetBudgetHandler> localizer)
    {
        _budgetRepository = budgetRepository;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _localizer = localizer;
    }

    public async Task<BudgetDto> Handle(GetBudgetRequest request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(request.Id, cancellationToken);
        if (budget == null)
        {
            throw new NotFoundException(_localizer["Budget with ID {0} not found.", request.Id]);
        }

        var account = await _accountRepository.GetByIdAsync(budget.AccountId, cancellationToken);
        if (account == null)
        {
            // Data integrity issue if account is null for an existing budget
            throw new NotFoundException(_localizer["Account associated with Budget ID {0} not found.", budget.Id]);
        }

        var transactions = await _transactionRepository.ListAsync(
            new TransactionsForAccountInPeriodSpec(budget.AccountId, budget.PeriodStartDate, budget.PeriodEndDate),
            cancellationToken);

        decimal actualAmount = 0;
        foreach (var trans in transactions)
        {
            // Actual amount calculation depends on the nature of the account and transaction type
            // For Expense accounts (common for budgeting): Debits increase expense, Credits decrease.
            // For Revenue accounts: Credits increase revenue, Debits decrease.
            // This logic assumes budget tracking for Expense accounts primarily.
            if (account.AccountType == AccountType.Expense || account.AccountType == AccountType.Asset)
            {
                if (trans.TransactionType == TransactionType.Debit) actualAmount += trans.Amount;
                else actualAmount -= trans.Amount; // Credits reduce expense/asset value
            }
            else if (account.AccountType == AccountType.Revenue || account.AccountType == AccountType.Liability || account.AccountType == AccountType.Equity)
            {
                if (trans.TransactionType == TransactionType.Credit) actualAmount += trans.Amount;
                else actualAmount -= trans.Amount; // Debits reduce revenue/liability/equity value
            }
        }

        var dto = budget.Adapt<BudgetDto>();
        dto.AccountName = account.AccountName;
        dto.ActualAmount = actualAmount;
        dto.Variance = dto.Amount - dto.ActualAmount;

        return dto;
    }
}

public class SearchBudgetsHandler : IRequestHandler<SearchBudgetsRequest, PaginationResponse<BudgetDto>>
{
    private readonly IRepository<Budget> _budgetRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IStringLocalizer<SearchBudgetsHandler> _localizer;

    public SearchBudgetsHandler(
        IRepository<Budget> budgetRepository,
        IRepository<Account> accountRepository,
        IRepository<Transaction> transactionRepository,
        IStringLocalizer<SearchBudgetsHandler> localizer)
    {
        _budgetRepository = budgetRepository;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _localizer = localizer;
    }

    public async Task<PaginationResponse<BudgetDto>> Handle(SearchBudgetsRequest request, CancellationToken cancellationToken)
    {
        var spec = new BudgetsBySearchFilterSpec(request);
        var budgets = await _budgetRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _budgetRepository.CountAsync(spec, cancellationToken);

        var budgetDtos = new List<BudgetDto>();
        foreach (var budget in budgets)
        {
            var account = await _accountRepository.GetByIdAsync(budget.AccountId, cancellationToken);
            if (account == null)
            {
                // Log or handle missing account for a budget
                // For now, skip adding AccountName or throw if strict consistency is required
                continue;
            }

            var transactions = await _transactionRepository.ListAsync(
                new TransactionsForAccountInPeriodSpec(budget.AccountId, budget.PeriodStartDate, budget.PeriodEndDate),
                cancellationToken);

            decimal actualAmount = 0;
            if (account.AccountType == AccountType.Expense || account.AccountType == AccountType.Asset)
            {
                actualAmount = transactions.Sum(t => t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount);
            }
            else if (account.AccountType == AccountType.Revenue || account.AccountType == AccountType.Liability || account.AccountType == AccountType.Equity)
            {
                actualAmount = transactions.Sum(t => t.TransactionType == TransactionType.Credit ? t.Amount : -t.Amount);
            }


            var dto = budget.Adapt<BudgetDto>();
            dto.AccountName = account.AccountName;
            dto.ActualAmount = actualAmount;
            dto.Variance = dto.Amount - dto.ActualAmount;
            budgetDtos.Add(dto);
        }

        return new PaginationResponse<BudgetDto>(budgetDtos, totalCount, request.PageNumber, request.PageSize);
    }
}

public class DeleteBudgetHandler : IRequestHandler<DeleteBudgetRequest, Guid>
{
    private readonly IRepository<Budget> _budgetRepository;
    private readonly IStringLocalizer<DeleteBudgetHandler> _localizer;
    private readonly ILogger<DeleteBudgetHandler> _logger;

    public DeleteBudgetHandler(
        IRepository<Budget> budgetRepository,
        IStringLocalizer<DeleteBudgetHandler> localizer,
        ILogger<DeleteBudgetHandler> logger)
    {
        _budgetRepository = budgetRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(DeleteBudgetRequest request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(request.Id, cancellationToken);
        if (budget == null)
        {
            throw new NotFoundException(_localizer["Budget with ID {0} not found.", request.Id]);
        }

        await _budgetRepository.DeleteAsync(budget, cancellationToken);

        _logger.LogInformation(_localizer["Budget deleted: {0}"], budget.Id);
        return budget.Id;
    }
}

// Specifications
public class BudgetsBySearchFilterSpec : EntitiesByPaginationFilterSpec<Budget, BudgetDto> // Assuming BudgetDto for direct mapping if possible
{
    public BudgetsBySearchFilterSpec(SearchBudgetsRequest request)
        : base(request) // Pass the request to the base constructor
    {
        Query.OrderBy(b => b.BudgetName, !request.HasOrderBy()); // Default order

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            Query.Where(b => b.BudgetName.Contains(request.Keyword));
        }

        if (request.AccountId.HasValue)
        {
            Query.Where(b => b.AccountId == request.AccountId.Value);
        }

        // Date filtering: budgets whose periods overlap with the given FromDate/ToDate range
        if (request.FromDate.HasValue)
        {
            Query.Where(b => b.PeriodEndDate >= request.FromDate.Value);
        }
        if (request.ToDate.HasValue)
        {
            Query.Where(b => b.PeriodStartDate <= request.ToDate.Value);
        }
    }
}

// Re-use or ensure this spec is available (from JournalEntry or Ledger services)
// If it's identical, it could be in a common Application/Accounting location.
// For now, defining it here for clarity if it's specific to Budget context needs.
public class TransactionsForAccountInPeriodSpec : Specification<Transaction>
{
    public TransactionsForAccountInPeriodSpec(Guid accountId, DateTime fromDate, DateTime toDate)
    {
        // Ensure transactions are within the budget period and are posted
        Query
            .Where(t => t.AccountId == accountId &&
                        t.JournalEntry.IsPosted && // Only consider posted transactions
                        t.JournalEntry.PostedDate >= fromDate &&
                        t.JournalEntry.PostedDate < toDate.AddDays(1)) // Up to the end of PeriodEndDate
            .Include(t => t.JournalEntry); // To access PostedDate
    }
}

// This mapping might be defined globally or in a specific Mapster configuration file.
// For now, this is a reminder of how it would look.
// public class BudgetMappingConfig : IRegister
// {
//     public void Register(TypeAdapterConfig config)
//     {
//         config.NewConfig<Budget, BudgetDto>()
//             .Map(dest => dest.AccountName, src => MapContext.Current!.GetService<IRepository<Account>>().GetByIdAsync(src.AccountId).Result.AccountName) // Example of complex mapping
//             // ActualAmount and Variance are calculated in handlers.
//             ;
//     }
// }
