using FluentAssertions;
using FSH.WebApi.Application.Accounting.Accounts;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Common.Contracts; // For IAggregateRoot
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Mapster; // Required for Adapt
using Ardalis.Specification; // Required for ISpecification
using FluentValidation;

namespace FSH.WebApi.Application.Tests.Accounting.Accounts;

public class AccountHandlerTests
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IStringLocalizer<CreateAccountHandler> _createLocalizer;
    private readonly ILogger<CreateAccountHandler> _createLogger;
    private readonly IStringLocalizer<UpdateAccountHandler> _updateLocalizer;
    private readonly ILogger<UpdateAccountHandler> _updateLogger;
    private readonly IStringLocalizer<GetAccountHandler> _getLocalizer;
    private readonly IStringLocalizer<SearchAccountsHandler> _searchLocalizer;

    public AccountHandlerTests()
    {
        _accountRepository = Substitute.For<IRepository<Account>>();
        _createLocalizer = Substitute.For<IStringLocalizer<CreateAccountHandler>>();
        _createLogger = Substitute.For<ILogger<CreateAccountHandler>>();
        _updateLocalizer = Substitute.For<IStringLocalizer<UpdateAccountHandler>>();
        _updateLogger = Substitute.For<ILogger<UpdateAccountHandler>>();
        _getLocalizer = Substitute.For<IStringLocalizer<GetAccountHandler>>();
        _searchLocalizer = Substitute.For<IStringLocalizer<SearchAccountsHandler>>();

        // Setup default localization messages if needed, e.g., _getLocalizer["Account not found."].Returns(new LocalizedString("Account not found.", "Account not found."));
    }

    // === CreateAccountHandler Tests ===
    [Fact]
    public async Task CreateAccountHandler_Should_Create_And_Return_AccountId_When_Request_Is_Valid()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            AccountName = "Test Account",
            AccountNumber = "ACC001",
            AccountType = AccountType.Asset.ToString(),
            InitialBalance = 100,
            Description = "Test Description"
        };

        _accountRepository.FirstOrDefaultAsync(Arg.Any<AccountByAccountNumberSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(null)); // Simulate account number is unique

        Account? capturedAccount = null;
        await _accountRepository.AddAsync(Arg.Do<Account>(acc => capturedAccount = acc), Arg.Any<CancellationToken>());

        var handler = new CreateAccountHandler(_accountRepository, _createLocalizer, _createLogger);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _accountRepository.Received(1).AddAsync(Arg.Is<Account>(a =>
            a.AccountName == request.AccountName &&
            a.AccountNumber == request.AccountNumber &&
            a.AccountType.ToString() == request.AccountType &&
            a.Balance == request.InitialBalance &&
            a.Description == request.Description &&
            a.IsActive == true
        ), Arg.Any<CancellationToken>());

        capturedAccount.Should().NotBeNull();
        capturedAccount!.Id.Should().Be(result);
    }

    [Fact]
    public async Task CreateAccountHandler_Should_Throw_ConflictException_When_AccountNumber_Already_Exists()
    {
        // Arrange
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = "EXISTING001", AccountType = "Asset" };
        var existingAccount = new Account("Existing", "EXISTING001", AccountType.Asset, 0, null);

        _accountRepository.FirstOrDefaultAsync(Arg.Any<AccountByAccountNumberSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(existingAccount));

        var handler = new CreateAccountHandler(_accountRepository, _createLocalizer, _createLogger);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
        await _accountRepository.DidNotReceive().AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAccountHandler_Should_Throw_ValidationException_When_AccountType_Is_Invalid()
    {
        // Arrange
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = "ACC002", AccountType = "InvalidType" };
        var handler = new CreateAccountHandler(_accountRepository, _createLocalizer, _createLogger);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === UpdateAccountHandler Tests ===
    [Fact]
    public async Task UpdateAccountHandler_Should_Update_Account_When_Request_Is_Valid()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var request = new UpdateAccountRequest
        {
            Id = accountId,
            AccountName = "Updated Name",
            AccountNumber = "UPDACC001",
            AccountType = AccountType.Liability.ToString(),
            Description = "Updated Desc",
            IsActive = false
        };
        var existingAccount = new Account("Old Name", "OLDACC001", AccountType.Asset, 100, "Old Desc", true);
        // Manually set Id because constructor doesn't take it
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(existingAccount, accountId);


        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(existingAccount));
        _accountRepository.FirstOrDefaultAsync(Arg.Any<AccountByAccountNumberSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(null)); // Simulate new account number is unique

        var handler = new UpdateAccountHandler(_accountRepository, _updateLocalizer, _updateLogger);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(accountId);
        await _accountRepository.Received(1).UpdateAsync(Arg.Is<Account>(a =>
            a.Id == accountId &&
            a.AccountName == request.AccountName &&
            a.AccountNumber == request.AccountNumber &&
            a.AccountType.ToString() == request.AccountType &&
            a.Description == request.Description &&
            a.IsActive == request.IsActive
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAccountHandler_Should_Throw_NotFoundException_When_Account_Does_Not_Exist()
    {
        // Arrange
        var request = new UpdateAccountRequest { Id = Guid.NewGuid() };
        _accountRepository.GetByIdAsync(request.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(null));
        var handler = new UpdateAccountHandler(_accountRepository, _updateLocalizer, _updateLogger);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAccountHandler_Should_Throw_ConflictException_When_Updated_AccountNumber_Conflicts_With_Another_Account()
    {
        // Arrange
        var accountToUpdateId = Guid.NewGuid();
        var conflictingAccountId = Guid.NewGuid();
        var request = new UpdateAccountRequest { Id = accountToUpdateId, AccountNumber = "CONFLICT001" };

        var accountToUpdate = new Account("Original Name", "ORIG001", AccountType.Asset, 0, null);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(accountToUpdate, accountToUpdateId);

        var conflictingAccount = new Account("Conflicting Account", "CONFLICT001", AccountType.Expense, 0, null);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(conflictingAccount, conflictingAccountId);

        _accountRepository.GetByIdAsync(accountToUpdateId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(accountToUpdate));
        _accountRepository.FirstOrDefaultAsync(Arg.Is<AccountByAccountNumberSpec>(spec => spec.AccountNumber == "CONFLICT001"), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(conflictingAccount));

        var handler = new UpdateAccountHandler(_accountRepository, _updateLocalizer, _updateLogger);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === GetAccountHandler Tests ===
    [Fact]
    public async Task GetAccountHandler_Should_Return_AccountDto_When_Account_Exists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new Account("Test Account", "ACC001", AccountType.Asset, 100m, "Test Desc", true);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(account, accountId); // Set Id via reflection

        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(account));

        var handler = new GetAccountHandler(_accountRepository, _getLocalizer);
        var request = new GetAccountRequest(accountId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(accountId);
        result.AccountName.Should().Be("Test Account");
        result.AccountNumber.Should().Be("ACC001");
        result.AccountType.Should().Be(AccountType.Asset.ToString());
        result.Balance.Should().Be(100m);
        result.Description.Should().Be("Test Desc");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetAccountHandler_Should_Throw_NotFoundException_When_Account_Does_Not_Exist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(null));
        var handler = new GetAccountHandler(_accountRepository, _getLocalizer);
        var request = new GetAccountRequest(accountId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === SearchAccountsHandler Tests ===
    [Fact]
    public async Task SearchAccountsHandler_Should_Return_PaginationResponse_Of_AccountDto()
    {
        // Arrange
        var request = new SearchAccountsRequest { Keyword = "Test", PageNumber = 1, PageSize = 10 };
        var accountsList = new List<Account>
        {
            new Account("Test Account 1", "T001", AccountType.Asset, 100, null),
            new Account("Another Test", "T002", AccountType.Expense, 50, null)
        };
        // Set TenantId if your spec/query relies on it, BaseDbContext usually handles this on save.
        // For this test, ensure the spec doesn't filter by TenantId unless explicitly part of the request.

        // Mock ListAsync and CountAsync
        _accountRepository.ListAsync(Arg.Any<ISpecification<Account, AccountDto>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(accountsList.Adapt<List<AccountDto>>())); // Map to DTO list for the handler
        _accountRepository.CountAsync(Arg.Any<ISpecification<Account, AccountDto>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(accountsList.Count));

        var handler = new SearchAccountsHandler(_accountRepository, _searchLocalizer);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(accountsList.Count);
        result.TotalCount.Should().Be(accountsList.Count);
        result.Data[0].AccountName.Should().Be("Test Account 1");
        result.Data[0].AccountType.Should().Be(AccountType.Asset.ToString()); // Verify enum to string mapping
    }
}

// Minimal AccountByAccountNumberSpec for tests if not accessible or to simplify
// In a real scenario, ensure the actual spec is used or accurately represented.
internal class AccountByAccountNumberSpec : Specification<Account>, ISingleResultSpecification
{
    public string AccountNumber { get; }
    public AccountByAccountNumberSpec(string accountNumber)
    {
        AccountNumber = accountNumber; // Store for assertion, not used in Query() here
        Query.Where(a => a.AccountNumber == accountNumber);
    }
}
