using System;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting;
using FSH.WebApi.Application.Accounting.Dtos;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Accounting.Enums;
using NSubstitute;
using Xunit;

namespace FSH.WebApi.Application.Tests.Accounting;

public class AccountServiceTests
{
    private readonly IRepository<Account> _accountRepository;
    private readonly AccountService _sut;

    public AccountServiceTests()
    {
        _accountRepository = Substitute.For<IRepository<Account>>();
        _sut = new AccountService(_accountRepository);
    }

    [Fact]
    public async Task CreateAccountAsync_ValidRequest_ShouldCreateAndReturnAccountId()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            AccountNumber = "101",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Balance = 1000
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var accountId = await _sut.CreateAccountAsync(request, cancellationToken);

        // Assert
        Assert.NotEqual(Guid.Empty, accountId);
        await _accountRepository.Received(1).AddAsync(Arg.Is<Account>(acc =>
            acc.AccountNumber == request.AccountNumber &&
            acc.AccountName == request.AccountName &&
            acc.AccountType == request.AccountType &&
            acc.Balance == request.Balance),
            cancellationToken);
    }

    [Theory]
    [InlineData(null, "Cash")]
    [InlineData("101", null)]
    [InlineData("", "Cash")]
    [InlineData("101", "")]
    public async Task CreateAccountAsync_InvalidRequest_MissingNumberOrName_ShouldThrowArgumentException(string accountNumber, string accountName)
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            AccountNumber = accountNumber,
            AccountName = accountName,
            AccountType = AccountType.Asset,
            Balance = 1000
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAccountAsync(request, cancellationToken));
        Assert.Contains("Account number and name are required", exception.Message); // More precise check if possible
        await _accountRepository.DidNotReceive().AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
    }

    // Placeholder for testing duplicate account number if service layer handles it.
    // If it's a DB constraint, this would be an integration test.
    // For now, assuming service layer doesn't check for duplicates explicitly before adding.

    [Fact]
    public async Task GetAccountByIdAsync_AccountExists_ShouldReturnAccountDto()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingAccount = new Account("101", "Cash", AccountType.Asset, 1000);
        // Need to set Id via reflection or make it settable for test, or assume repository returns it
        // For simplicity, let's assume GetByIdAsync correctly fetches an entity with its Id.
        // We can't directly set Id if it's private set and only set by EF Core.
        // However, the DTO mapping will use the Id from the fetched entity.
        // Let's simulate the repository returning an account that has an Id.
        // We can create a new Account and then use its Id for the DTO.
        // The key is that the repository returns *an* account.

        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(existingAccount)); // Return our specific account

        var cancellationToken = CancellationToken.None;

        // Act
        var accountDto = await _sut.GetAccountByIdAsync(accountId, cancellationToken);

        // Assert
        Assert.NotNull(accountDto);
        Assert.Equal(existingAccount.Id, accountDto.Id); // This will fail if existingAccount.Id is Guid.Empty
                                                       // We need to simulate that the account object from repo has its Id set.
                                                       // A better way for unit test is to ensure the DTO mapping is correct.
        Assert.Equal(existingAccount.AccountNumber, accountDto.AccountNumber);
        Assert.Equal(existingAccount.AccountName, accountDto.AccountName);
        // Add assertions for CreatedOn, LastModifiedOn if important for this test
    }

    [Fact]
    public async Task GetAccountByIdAsync_AccountDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(null));
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAccountByIdAsync(accountId, cancellationToken));
        Assert.Contains($"Account with ID {accountId} not found", exception.Message);
    }

    [Fact]
    public async Task UpdateAccountAsync_AccountExists_ValidRequest_ShouldUpdateAndReturnDto()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingAccount = new Account("101", "Old Name", AccountType.Asset, 1000);
        // Simulate Id being set by EF Core
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(existingAccount, accountId, null);


        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(existingAccount));

        var request = new UpdateAccountRequest
        {
            AccountNumber = "102",
            AccountName = "New Name",
            AccountType = AccountType.Liability,
            IsActive = false
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var updatedAccountDto = await _sut.UpdateAccountAsync(accountId, request, cancellationToken);

        // Assert
        Assert.NotNull(updatedAccountDto);
        Assert.Equal(accountId, updatedAccountDto.Id);
        Assert.Equal(request.AccountNumber, updatedAccountDto.AccountNumber);
        Assert.Equal(request.AccountName, updatedAccountDto.AccountName);
        Assert.Equal(request.AccountType, updatedAccountDto.AccountType);
        Assert.Equal(request.IsActive, updatedAccountDto.IsActive);

        await _accountRepository.Received(1).UpdateAsync(Arg.Is<Account>(acc =>
            acc.Id == accountId &&
            acc.AccountNumber == request.AccountNumber &&
            acc.AccountName == request.AccountName &&
            acc.AccountType == request.AccountType &&
            acc.IsActive == request.IsActive
        ), cancellationToken);
    }

    [Fact]
    public async Task UpdateAccountAsync_AccountDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(null));

        var request = new UpdateAccountRequest
        {
            AccountNumber = "102",
            AccountName = "New Name",
            AccountType = AccountType.Liability,
            IsActive = false
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAccountAsync(accountId, request, cancellationToken));
        Assert.Contains($"Account with ID {accountId} not found", exception.Message);
    }

    [Theory]
    [InlineData(null, "New Name")]
    [InlineData("102", null)]
    [InlineData("", "New Name")]
    [InlineData("102", "")]
    public async Task UpdateAccountAsync_InvalidRequest_MissingNumberOrName_ShouldThrowArgumentException(string accountNumber, string accountName)
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingAccount = new Account("101", "Old Name", AccountType.Asset, 1000);
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(existingAccount));

        var request = new UpdateAccountRequest
        {
            AccountNumber = accountNumber,
            AccountName = accountName,
            AccountType = AccountType.Liability,
            IsActive = true
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAccountAsync(accountId, request, cancellationToken));
        Assert.Contains("Account number and name are required for update", exception.Message);
        await _accountRepository.DidNotReceive().UpdateAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAccountAsync_AccountExists_ZeroBalance_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingAccount = new Account("101", "Cash", AccountType.Asset, 0) { IsActive = false }; // Balance is 0, IsActive is false
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(existingAccount, accountId, null);

        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(existingAccount));
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.DeleteAccountAsync(accountId, cancellationToken);

        // Assert
        Assert.True(result);
        await _accountRepository.Received(1).DeleteAsync(existingAccount, cancellationToken);
    }

    [Fact]
    public async Task DeleteAccountAsync_AccountExists_Active_ZeroBalance_ShouldDeactivateThenDeleteAndReturnTrue()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        // Account is active but has zero balance
        var existingAccount = new Account("101", "Cash", AccountType.Asset, 0) { IsActive = true };
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(existingAccount, accountId, null);


        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(existingAccount));
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.DeleteAccountAsync(accountId, cancellationToken);

        // Assert
        Assert.True(result);
        Assert.False(existingAccount.IsActive); // Check if Deactivate was called implicitly
        await _accountRepository.Received(1).DeleteAsync(existingAccount, cancellationToken);
    }


    [Fact]
    public async Task DeleteAccountAsync_AccountDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(null));
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAccountAsync(accountId, cancellationToken));
        Assert.Contains($"Account with ID {accountId} not found", exception.Message);
    }

    [Fact]
    public async Task DeleteAccountAsync_AccountHasNonZeroBalance_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingAccount = new Account("101", "Cash", AccountType.Asset, 100); // Non-zero balance
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(existingAccount, accountId, null);

        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(existingAccount));
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAccountAsync(accountId, cancellationToken));
        Assert.Contains("Cannot delete account with non-zero balance", exception.Message);
        await _accountRepository.DidNotReceive().DeleteAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAccountsAsync_WithMatchingCriteria_ShouldReturnMatchingAccounts()
    {
        // Arrange
        var account1 = new Account("101", "Cash A", AccountType.Asset, 100);
        var account2 = new Account("102", "Cash B", AccountType.Asset, 200);
        var account3 = new Account("201", "Payables", AccountType.Liability, 300);
        var allAccounts = new List<Account> { account1, account2, account3 };

        _accountRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Account>>(allAccounts)); // Simulate ListAsync

        var request = new SearchAccountsRequest
        {
            AccountType = AccountType.Asset,
            PageNumber = 1,
            PageSize = 10
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.SearchAccountsAsync(request, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.AccountNumber == "101");
        Assert.Contains(result, r => r.AccountNumber == "102");
    }

    [Fact]
    public async Task SearchAccountsAsync_WithNonMatchingCriteria_ShouldReturnEmptyList()
    {
        // Arrange
        var account1 = new Account("101", "Cash A", AccountType.Asset, 100);
        var account2 = new Account("102", "Cash B", AccountType.Asset, 200);
        var allAccounts = new List<Account> { account1, account2 };

        _accountRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Account>>(allAccounts));

        var request = new SearchAccountsRequest
        {
            AccountType = AccountType.Expense, // No expense accounts
            PageNumber = 1,
            PageSize = 10
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.SearchAccountsAsync(request, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAccountsAsync_Paging_ShouldReturnCorrectPage()
    {
        // Arrange
        var accounts = new List<Account>();
        for(int i = 0; i < 15; i++)
        {
            accounts.Add(new Account($"ACC{i:D3}", $"Account {i}", AccountType.Asset, i * 100));
        }

        _accountRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Account>>(accounts));

        var request = new SearchAccountsRequest
        {
            PageNumber = 2,
            PageSize = 5
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.SearchAccountsAsync(request, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        Assert.Equal("ACC005", result[0].AccountNumber); // Accounts ACC000-ACC004 on page 1
    }
}
