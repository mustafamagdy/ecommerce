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
using System.Collections.Generic; // Required for List

namespace FSH.WebApi.Application.Accounting.JournalEntries;

// Using JournalEntryService as the class name to contain all handlers for JournalEntries.
public class CreateJournalEntryHandler : IRequestHandler<CreateJournalEntryRequest, Guid>
{
    private readonly IRepository<JournalEntry> _journalEntryRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IStringLocalizer<CreateJournalEntryHandler> _localizer;
    private readonly ILogger<CreateJournalEntryHandler> _logger;

    public CreateJournalEntryHandler(
        IRepository<JournalEntry> journalEntryRepository,
        IRepository<Account> accountRepository,
        IStringLocalizer<CreateJournalEntryHandler> localizer,
        ILogger<CreateJournalEntryHandler> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateJournalEntryRequest request, CancellationToken cancellationToken)
    {
        // Validate all AccountIds exist
        foreach (var item in request.Transactions)
        {
            var account = await _accountRepository.GetByIdAsync(item.AccountId, cancellationToken);
            if (account == null)
            {
                throw new NotFoundException(_localizer["Account with ID {0} not found.", item.AccountId]);
            }
            if(!account.IsActive)
            {
                throw new ValidationException(_localizer["Account with ID {0} is not active.", item.AccountId]);
            }
        }

        var journalEntry = new JournalEntry(
            entryDate: request.EntryDate,
            description: request.Description,
            referenceNumber: request.ReferenceNumber
        );

        foreach (var item in request.Transactions)
        {
            if (!Enum.TryParse<TransactionType>(item.TransactionType, true, out var transactionTypeEnum))
            {
                // This should ideally be caught by FluentValidation earlier
                throw new ValidationException(_localizer["Invalid Transaction Type for account {0}.", item.AccountId]);
            }

            var transaction = new Transaction(
                journalEntryId: journalEntry.Id, // Will be set by EF Core if relationship is defined
                accountId: item.AccountId,
                transactionType: transactionTypeEnum,
                amount: item.Amount,
                description: item.Description
            );
            journalEntry.AddTransaction(transaction);
        }

        await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);

        _logger.LogInformation(_localizer["Journal Entry created: {0}"], journalEntry.Id);
        return journalEntry.Id;
    }
}

public class PostJournalEntryHandler : IRequestHandler<PostJournalEntryRequest, Guid>
{
    private readonly IRepository<JournalEntry> _journalEntryRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IStringLocalizer<PostJournalEntryHandler> _localizer;
    private readonly ILogger<PostJournalEntryHandler> _logger;
    // In a more complex system, an IAccountUpdaterService might be used.
    // For now, logic is within the handler.

    public PostJournalEntryHandler(
        IRepository<JournalEntry> journalEntryRepository,
        IRepository<Account> accountRepository,
        IStringLocalizer<PostJournalEntryHandler> localizer,
        ILogger<PostJournalEntryHandler> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(PostJournalEntryRequest request, CancellationToken cancellationToken)
    {
        var journalEntry = await _journalEntryRepository.FirstOrDefaultAsync(new JournalEntryWithTransactionsSpec(request.JournalEntryId), cancellationToken);

        if (journalEntry == null)
        {
            throw new NotFoundException(_localizer["Journal Entry with ID {0} not found.", request.JournalEntryId]);
        }

        if (journalEntry.IsPosted)
        {
            throw new ValidationException(_localizer["Journal Entry {0} is already posted.", request.JournalEntryId]);
        }

        if (!journalEntry.Transactions.Any())
        {
            throw new ValidationException(_localizer["Journal Entry {0} has no transactions to post.", request.JournalEntryId]);
        }

        // Ensure debits equal credits one last time (though validator should catch this)
        decimal totalDebits = journalEntry.Transactions.Where(t => t.TransactionType == TransactionType.Debit).Sum(t => t.Amount);
        decimal totalCredits = journalEntry.Transactions.Where(t => t.TransactionType == TransactionType.Credit).Sum(t => t.Amount);

        if (totalDebits != totalCredits)
        {
            // This indicates a data integrity issue if CreateJournalEntryRequestValidator passed
            _logger.LogError(_localizer["Journal Entry {0} debits do not equal credits. Debits: {1}, Credits: {2}"], request.JournalEntryId, totalDebits, totalCredits);
            throw new InvalidOperationException(_localizer["Debits do not equal Credits for Journal Entry {0}. Cannot post.", request.JournalEntryId]);
        }


        foreach (var transaction in journalEntry.Transactions)
        {
            var account = await _accountRepository.GetByIdAsync(transaction.AccountId, cancellationToken);
            if (account == null) // Should not happen if Create validation was correct
            {
                throw new NotFoundException(_localizer["Account with ID {0} not found while posting Journal Entry {1}.", transaction.AccountId, journalEntry.Id]);
            }

            decimal amountChange = transaction.Amount;

            switch (account.AccountType)
            {
                case AccountType.Asset:
                case AccountType.Expense:
                    if (transaction.TransactionType == TransactionType.Debit)
                        account.Update(accountName: null, accountNumber: null, accountType: null, balance: account.Balance + amountChange, description: null, isActive: null);
                    else // Credit
                        account.Update(accountName: null, accountNumber: null, accountType: null, balance: account.Balance - amountChange, description: null, isActive: null);
                    break;

                case AccountType.Liability:
                case AccountType.Equity:
                case AccountType.Revenue:
                    if (transaction.TransactionType == TransactionType.Credit)
                        account.Update(accountName: null, accountNumber: null, accountType: null, balance: account.Balance + amountChange, description: null, isActive: null);
                    else // Debit
                        account.Update(accountName: null, accountNumber: null, accountType: null, balance: account.Balance - amountChange, description: null, isActive: null);
                    break;

                default: // Should not happen
                    throw new InvalidOperationException(_localizer["Unknown account type for account {0}", account.Id]);
            }
            await _accountRepository.UpdateAsync(account, cancellationToken);
        }

        journalEntry.Post(); // Sets IsPosted = true and PostedDate
        await _journalEntryRepository.UpdateAsync(journalEntry, cancellationToken);

        _logger.LogInformation(_localizer["Journal Entry posted: {0}"], journalEntry.Id);
        return journalEntry.Id;
    }
}

public class GetJournalEntryHandler : IRequestHandler<GetJournalEntryRequest, JournalEntryDto>
{
    private readonly IRepository<JournalEntry> _repository;
    private readonly IStringLocalizer<GetJournalEntryHandler> _localizer;
    private readonly IRepository<Account> _accountRepository; // To fetch AccountName for TransactionDto

    public GetJournalEntryHandler(IRepository<JournalEntry> repository, IStringLocalizer<GetJournalEntryHandler> localizer, IRepository<Account> accountRepository)
    {
        _repository = repository;
        _localizer = localizer;
        _accountRepository = accountRepository;
    }

    public async Task<JournalEntryDto> Handle(GetJournalEntryRequest request, CancellationToken cancellationToken)
    {
        var journalEntry = await _repository.FirstOrDefaultAsync(new JournalEntryWithTransactionsSpec(request.Id), cancellationToken);

        if (journalEntry == null)
        {
            throw new NotFoundException(_localizer["Journal Entry with ID {0} not found.", request.Id]);
        }

        var dto = journalEntry.Adapt<JournalEntryDto>();

        // Populate AccountName in TransactionDtos
        foreach (var transactionDto in dto.Transactions)
        {
            var account = await _accountRepository.GetByIdAsync(transactionDto.AccountId, cancellationToken);
            transactionDto.AccountName = account?.AccountName; // Set AccountName, will be null if account somehow not found
            transactionDto.TransactionType = journalEntry.Transactions
                .First(t => t.Id == transactionDto.Id).TransactionType.ToString();
        }
        dto.IsPosted = journalEntry.IsPosted; // Ensure this is mapped correctly

        return dto;
    }
}

public class SearchJournalEntriesHandler : IRequestHandler<SearchJournalEntriesRequest, PaginationResponse<JournalEntryDto>>
{
    private readonly IRepository<JournalEntry> _repository;
    private readonly IStringLocalizer<SearchJournalEntriesHandler> _localizer;
    private readonly IRepository<Account> _accountRepository; // For populating AccountName in DTOs

    public SearchJournalEntriesHandler(IRepository<JournalEntry> repository, IStringLocalizer<SearchJournalEntriesHandler> localizer, IRepository<Account> accountRepository)
    {
        _repository = repository;
        _localizer = localizer;
        _accountRepository = accountRepository;
    }

    public async Task<PaginationResponse<JournalEntryDto>> Handle(SearchJournalEntriesRequest request, CancellationToken cancellationToken)
    {
        var spec = new JournalEntriesBySearchFilterSpec(request);
        var journalEntries = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var dtos = new List<JournalEntryDto>();
        foreach (var je in journalEntries)
        {
            var dto = je.Adapt<JournalEntryDto>();
            dto.IsPosted = je.IsPosted; // Ensure mapping
            foreach (var transactionDto in dto.Transactions)
            {
                var account = await _accountRepository.GetByIdAsync(transactionDto.AccountId, cancellationToken);
                transactionDto.AccountName = account?.AccountName;
                // Ensure TransactionType is string
                transactionDto.TransactionType = je.Transactions
                    .First(t => t.Id == transactionDto.Id).TransactionType.ToString();
            }
            dtos.Add(dto);
        }

        return new PaginationResponse<JournalEntryDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}

// Specifications
public class JournalEntryWithTransactionsSpec : Specification<JournalEntry>, ISingleResultSpecification
{
    public JournalEntryWithTransactionsSpec(Guid journalEntryId) =>
        Query
            .Where(je => je.Id == journalEntryId)
            .Include(je => je.Transactions);
}

public class JournalEntriesBySearchFilterSpec : Specification<JournalEntry, JournalEntryDto>
{
    public JournalEntriesBySearchFilterSpec(SearchJournalEntriesRequest request)
        : base() // EntitiesByPaginationFilterSpec might not be suitable if it doesn't include child collections by default
    {
        Query.Include(je => je.Transactions) // Eager load transactions
             .OrderByDescending(je => je.EntryDate, !request.HasOrderBy()); // Default order

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            Query.Where(je => je.Description.Contains(request.Keyword) || (je.ReferenceNumber != null && je.ReferenceNumber.Contains(request.Keyword)));
        }

        if (request.StartDate.HasValue)
        {
            Query.Where(je => je.EntryDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            Query.Where(je => je.EntryDate <= request.EndDate.Value);
        }

        if (request.IsPosted.HasValue)
        {
            Query.Where(je => je.IsPosted == request.IsPosted.Value);
        }

        // Apply pagination if PaginationFilter properties are set
        if (request.PageNumber > 0 && request.PageSize > 0)
        {
            Query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);
        }
    }
}

// Mapping configuration (example, usually in a dedicated mapping profile)
// This is to guide Mapster for complex mappings if needed.
// For simple DTOs, Mapster often works by convention.
public class JournalEntryMappingConfig // : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<JournalEntry, JournalEntryDto>()
            .Map(dest => dest.Transactions, src => src.Transactions.Adapt<List<TransactionDto>>());

        config.NewConfig<Transaction, TransactionDto>()
            .Map(dest => dest.TransactionType, src => src.TransactionType.ToString());
            // AccountName is populated manually in handlers after fetching from AccountRepository
    }
}
