using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using FluentValidation;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Mailing;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Common.Contracts;
using Mapster;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging; // Assuming ILogger is used for logging

namespace FSH.WebApi.Application.Accounting.Accounts;

// Using AccountService as the class name to contain all handlers for Accounts as per typical CQRS/MediatR setup.

public class CreateAccountHandler : IRequestHandler<CreateAccountRequest, Guid>
{
    private readonly IRepository<Account> _repository;
    private readonly IStringLocalizer<CreateAccountHandler> _localizer; // For localization if needed
    private readonly ILogger<CreateAccountHandler> _logger; // For logging

    public CreateAccountHandler(IRepository<Account> repository, IStringLocalizer<CreateAccountHandler> localizer, ILogger<CreateAccountHandler> logger)
    {
        _repository = repository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        // Validate AccountType
        if (!Enum.TryParse<AccountType>(request.AccountType, true, out var accountTypeEnum))
        {
            throw new ValidationException(_localizer["Invalid Account Type."]);
        }

        // Check for duplicate AccountNumber
        var existingAccount = await _repository.FirstOrDefaultAsync(new AccountByAccountNumberSpec(request.AccountNumber), cancellationToken);
        if (existingAccount != null)
        {
            throw new ConflictException(_localizer["An account with this account number already exists."]);
        }

        var account = new Account(
            accountName: request.AccountName,
            accountNumber: request.AccountNumber,
            accountType: accountTypeEnum,
            balance: request.InitialBalance,
            description: request.Description,
            isActive: true // New accounts are active by default
        );

        await _repository.AddAsync(account, cancellationToken);

        _logger.LogInformation(_localizer["Account created: {0}"], account.Id);

        return account.Id;
    }
}

public class UpdateAccountHandler : IRequestHandler<UpdateAccountRequest, Guid>
{
    private readonly IRepository<Account> _repository;
    private readonly IStringLocalizer<UpdateAccountHandler> _localizer;
    private readonly ILogger<UpdateAccountHandler> _logger;

    public UpdateAccountHandler(IRepository<Account> repository, IStringLocalizer<UpdateAccountHandler> localizer, ILogger<UpdateAccountHandler> logger)
    {
        _repository = repository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (account == null)
        {
            throw new NotFoundException(_localizer["Account not found."]);
        }

        // Validate AccountType if provided
        AccountType? accountTypeEnum = null;
        if (request.AccountType is not null)
        {
            if (!Enum.TryParse<AccountType>(request.AccountType, true, out var parsedEnum))
            {
                throw new ValidationException(_localizer["Invalid Account Type."]);
            }
            accountTypeEnum = parsedEnum;
        }

        // Check for duplicate AccountNumber if it's being changed
        if (request.AccountNumber is not null && !account.AccountNumber.Equals(request.AccountNumber, StringComparison.OrdinalIgnoreCase))
        {
            var existingAccount = await _repository.FirstOrDefaultAsync(new AccountByAccountNumberSpec(request.AccountNumber), cancellationToken);
            if (existingAccount != null && existingAccount.Id != account.Id)
            {
                throw new ConflictException(_localizer["An account with this account number already exists."]);
            }
        }

        account.Update(
            accountName: request.AccountName,
            accountNumber: request.AccountNumber,
            accountType: accountTypeEnum,
            balance: null, // Balance updates should go through transactions, not directly here
            description: request.Description,
            isActive: request.IsActive
        );

        await _repository.UpdateAsync(account, cancellationToken);

        _logger.LogInformation(_localizer["Account updated: {0}"], account.Id);

        return account.Id;
    }
}

public class GetAccountHandler : IRequestHandler<GetAccountRequest, AccountDto>
{
    private readonly IRepository<Account> _repository;
    private readonly IStringLocalizer<GetAccountHandler> _localizer;

    public GetAccountHandler(IRepository<Account> repository, IStringLocalizer<GetAccountHandler> localizer)
    {
        _repository = repository;
        _localizer = localizer;
    }

    public async Task<AccountDto> Handle(GetAccountRequest request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (account == null)
        {
            throw new NotFoundException(_localizer["Account not found."]);
        }

        // Map to DTO
        var dto = account.Adapt<AccountDto>();
        dto.AccountType = account.AccountType.ToString(); // Enum to string mapping

        return dto;
    }
}

public class SearchAccountsHandler : IRequestHandler<SearchAccountsRequest, PaginationResponse<AccountDto>>
{
    private readonly IRepository<Account> _repository;
    private readonly IStringLocalizer<SearchAccountsHandler> _localizer;

    public SearchAccountsHandler(IRepository<Account> repository, IStringLocalizer<SearchAccountsHandler> localizer)
    {
        _repository = repository;
        _localizer = localizer;
    }

    public async Task<PaginationResponse<AccountDto>> Handle(SearchAccountsRequest request, CancellationToken cancellationToken)
    {
        var spec = new AccountsBySearchFilterSpec(request);
        var accounts = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var dtos = accounts.Adapt<List<AccountDto>>();
        // Ensure AccountType is string in DTO
        for(int i=0; i < accounts.Count; i++)
        {
            dtos[i].AccountType = accounts[i].AccountType.ToString();
        }


        return new PaginationResponse<AccountDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}

// Specification for checking duplicate account number
public class AccountByAccountNumberSpec : Specification<Account>, ISingleResultSpecification
{
    public AccountByAccountNumberSpec(string accountNumber) =>
        Query.Where(a => a.AccountNumber == accountNumber);
}

// Specification for searching and filtering accounts
public class AccountsBySearchFilterSpec : EntitiesByPaginationFilterSpec<Account, AccountDto>
{
    public AccountsBySearchFilterSpec(SearchAccountsRequest request)
        : base(request)
    {
        Query.OrderBy(a => a.AccountName, !request.HasOrderBy()); // Default order

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            Query.Where(a => a.AccountName.Contains(request.Keyword) || a.AccountNumber.Contains(request.Keyword));
        }

        if (!string.IsNullOrEmpty(request.AccountType))
        {
            if (Enum.TryParse<AccountType>(request.AccountType, true, out var accountTypeEnum))
            {
                Query.Where(a => a.AccountType == accountTypeEnum);
            }
            else
            {
                // Handle invalid enum string - perhaps log or throw, or just ignore
                // For now, ignoring if it's not a valid enum type
            }
        }
    }
}

// It's good practice to register mappings if not using global convention-based mapping
// For example, in a Startup or MappingProfile class:
// TypeAdapterConfig<Account, AccountDto>.NewConfig()
//    .Map(dest => dest.AccountType, src => src.AccountType.ToString());
// However, for this exercise, direct mapping in handlers or relying on Mapster's global settings is assumed.
// The direct mapping for AccountType enum to string is done in the handlers for clarity.

// Global usings might already cover some of these, but explicitly stating for clarity:
// using FSH.WebApi.Application.Common.Models; (for PaginationResponse)
// using FSH.WebApi.Domain.Accounting; (for Account, AccountType)
// using MediatR;
// using Ardalis.Specification;
// using Mapster;
// using FSH.WebApi.Application.Common.Persistence; (for IRepository)
// using FSH.WebApi.Application.Common.Exceptions; (for NotFoundException, ConflictException)
// using Microsoft.Extensions.Localization; (for IStringLocalizer)
// using Microsoft.Extensions.Logging; (for ILogger)
// using System.Threading;
// using System.Threading.Tasks;
// using System.Collections.Generic;
// using System.Linq;
// using System;
// using FluentValidation; // For ValidationException
// using FSH.WebApi.Domain.Common.Contracts; // For AuditableEntity (if base spec needs it)
// using FSH.WebApi.Application.Common.Specification; // For EntitiesByPaginationFilterSpec
