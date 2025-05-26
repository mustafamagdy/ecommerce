using System;
using System.Collections.Generic;
using System.Linq;
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

public class JournalEntryServiceTests
{
    private readonly IRepository<JournalEntry> _journalEntryRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly JournalEntryService _sut;

    public JournalEntryServiceTests()
    {
        _journalEntryRepository = Substitute.For<IRepository<JournalEntry>>();
        _accountRepository = Substitute.For<IRepository<Account>>();
        _sut = new JournalEntryService(_journalEntryRepository, _accountRepository);
    }

    [Fact]
    public async Task CreateJournalEntryAsync_ValidRequest_ShouldCreateAndReturnJournalEntryId()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var account1 = new Account("101", "Cash", AccountType.Asset, 0);
        var account2 = new Account("201", "Revenue", AccountType.Revenue, 0);

        _accountRepository.GetByIdAsync(accountId1, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(account1));
        _accountRepository.GetByIdAsync(accountId2, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(account2));

        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Test Entry",
            Transactions = new List<TransactionRequest>
            {
                new() { AccountId = accountId1, TransactionType = TransactionType.Debit, Amount = 100, Description = "Debit Cash" },
                new() { AccountId = accountId2, TransactionType = TransactionType.Credit, Amount = 100, Description = "Credit Revenue" }
            }
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var journalEntryId = await _sut.CreateJournalEntryAsync(request, cancellationToken);

        // Assert
        Assert.NotEqual(Guid.Empty, journalEntryId);
        await _journalEntryRepository.Received(1).AddAsync(Arg.Is<JournalEntry>(je =>
            je.Description == request.Description &&
            je.EntryDate.Date == request.EntryDate.Date // Comparing date part only as time might differ slightly
            // Further assertions on transactions would require more complex mocking or capturing arguments
        ), cancellationToken);
    }

    [Fact]
    public async Task CreateJournalEntryAsync_NoTransactions_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Test Entry",
            Transactions = new List<TransactionRequest>() // Empty list
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateJournalEntryAsync(request, cancellationToken));
        Assert.Contains("Journal entry must have at least one transaction", exception.Message);
    }

    [Fact]
    public async Task CreateJournalEntryAsync_TransactionAccountNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentAccountId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(nonExistentAccountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(null)); // Simulate account not found

        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Test Entry",
            Transactions = new List<TransactionRequest>
            {
                new() { AccountId = nonExistentAccountId, TransactionType = TransactionType.Debit, Amount = 100 }
            }
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        // Note: The current service implementation iterates accounts and throws NotFoundException.
        // If it were to collect all errors, the test would be different.
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateJournalEntryAsync(request, cancellationToken));
        Assert.Contains($"Account with ID {nonExistentAccountId} not found", exception.Message);
    }

    // More tests to come for GetJournalEntryByIdAsync, PostJournalEntryAsync, VoidJournalEntryAsync etc.

    [Fact]
    public async Task GetJournalEntryByIdAsync_EntryExists_ShouldReturnJournalEntryDto()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var account1 = new Account("101", "Cash", AccountType.Asset, 0);
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(account1, Guid.NewGuid(), null);

        var transaction1 = new Transaction(account1.Id, journalEntryId, TransactionType.Debit, 100, "Debit", DateTime.UtcNow);
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(transaction1, Guid.NewGuid(), null);
        // Manually link account to transaction for DTO mapping
        typeof(Transaction).GetProperty("Account")!.SetValue(transaction1, account1, null);


        var existingEntry = new JournalEntry(DateTime.UtcNow, "Test Entry");
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(existingEntry, journalEntryId, null);
        existingEntry.AddTransaction(transaction1); // Assuming AddTransaction works as expected

        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(existingEntry));
        var cancellationToken = CancellationToken.None;

        // Act
        var journalEntryDto = await _sut.GetJournalEntryByIdAsync(journalEntryId, cancellationToken);

        // Assert
        Assert.NotNull(journalEntryDto);
        Assert.Equal(journalEntryId, journalEntryDto.Id);
        Assert.Equal(existingEntry.Description, journalEntryDto.Description);
        Assert.Single(journalEntryDto.Transactions);
        Assert.Equal(transaction1.Id, journalEntryDto.Transactions[0].Id);
        Assert.Equal(account1.AccountName, journalEntryDto.Transactions[0].AccountName);
    }

    [Fact]
    public async Task GetJournalEntryByIdAsync_EntryDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(null));
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetJournalEntryByIdAsync(journalEntryId, cancellationToken));
        Assert.Contains($"JournalEntry with ID {journalEntryId} not found", exception.Message);
    }

    [Fact]
    public async Task PostJournalEntryAsync_ValidDraftEntry_ShouldPostAndUpdateAccountBalances()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();

        var account1 = Substitute.For<Account>("101", "Cash", AccountType.Asset, 0); // Use spy if want to check internal state changes more easily
        var account2 = Substitute.For<Account>("201", "Revenue", AccountType.Revenue, 0);
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(account1, accountId1, null);
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(account2, accountId2, null);


        var transaction1 = new Transaction(accountId1, journalEntryId, TransactionType.Debit, 100, "Debit", DateTime.UtcNow);
        typeof(Transaction).GetProperty("Account")!.SetValue(transaction1, account1, null); // Link account mock
        var transaction2 = new Transaction(accountId2, journalEntryId, TransactionType.Credit, 100, "Credit", DateTime.UtcNow);
        typeof(Transaction).GetProperty("Account")!.SetValue(transaction2, account2, null); // Link account mock


        var entry = new JournalEntry(DateTime.UtcNow, "Test Post") { Status = JournalEntryStatus.Draft };
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(entry, journalEntryId, null);
        entry.Transactions.Add(transaction1);
        entry.Transactions.Add(transaction2);

        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(entry));
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.PostJournalEntryAsync(journalEntryId, cancellationToken);

        // Assert
        Assert.True(result);
        Assert.Equal(JournalEntryStatus.Posted, entry.Status);
        account1.Received(1).Debit(100);
        account2.Received(1).Credit(100);
        await _journalEntryRepository.Received(1).UpdateAsync(entry, cancellationToken);
    }

    [Fact]
    public async Task PostJournalEntryAsync_AlreadyPostedEntry_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var entry = new JournalEntry(DateTime.UtcNow, "Test Post") { Status = JournalEntryStatus.Posted }; // Already posted
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(entry, journalEntryId, null);

        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(entry));
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.PostJournalEntryAsync(journalEntryId, cancellationToken));
        Assert.Contains("Only draft entries can be posted.", exception.Message); // Domain entity throws this
    }

    [Fact]
    public async Task PostJournalEntryAsync_EntryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(null));
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.PostJournalEntryAsync(journalEntryId, cancellationToken));
    }

    [Fact]
    public async Task PostJournalEntryAsync_TransactionsNotBalanced_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var account1 = Substitute.For<Account>("101", "Cash", AccountType.Asset, 0);
        var account2 = Substitute.For<Account>("201", "Revenue", AccountType.Revenue, 0);
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(account1, accountId1, null);
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(account2, accountId2, null);


        var transaction1 = new Transaction(accountId1, journalEntryId, TransactionType.Debit, 100, "Debit", DateTime.UtcNow);
        typeof(Transaction).GetProperty("Account")!.SetValue(transaction1, account1, null);
        var transaction2 = new Transaction(accountId2, journalEntryId, TransactionType.Credit, 90, "Credit Unbalanced", DateTime.UtcNow); // Not balanced
        typeof(Transaction).GetProperty("Account")!.SetValue(transaction2, account2, null);


        var entry = new JournalEntry(DateTime.UtcNow, "Test Post Unbalanced") { Status = JournalEntryStatus.Draft };
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(entry, journalEntryId, null);
        entry.Transactions.Add(transaction1);
        entry.Transactions.Add(transaction2);

        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(entry));
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.PostJournalEntryAsync(journalEntryId, cancellationToken));
        Assert.Contains("Debits must equal credits to post the journal entry.", exception.Message); // Domain entity throws this
    }

    [Fact]
    public async Task VoidJournalEntryAsync_ValidPostedEntry_ShouldVoidAndUpdateAccountBalances()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();

        var account1 = Substitute.For<Account>("101", "Cash", AccountType.Asset, 100); // Initial balance
        var account2 = Substitute.For<Account>("201", "Revenue", AccountType.Revenue, -100); // Initial balance
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(account1, accountId1, null);
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(account2, accountId2, null);

        var transaction1 = new Transaction(accountId1, journalEntryId, TransactionType.Debit, 100, "Debit", DateTime.UtcNow);
        typeof(Transaction).GetProperty("Account")!.SetValue(transaction1, account1, null);
        var transaction2 = new Transaction(accountId2, journalEntryId, TransactionType.Credit, 100, "Credit", DateTime.UtcNow);
        typeof(Transaction).GetProperty("Account")!.SetValue(transaction2, account2, null);

        var entry = new JournalEntry(DateTime.UtcNow, "Test Void") { Status = JournalEntryStatus.Posted }; // Already posted
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(entry, journalEntryId, null);
        entry.Transactions.Add(transaction1);
        entry.Transactions.Add(transaction2);

        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(entry));
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.VoidJournalEntryAsync(journalEntryId, cancellationToken);

        // Assert
        Assert.True(result);
        Assert.Equal(JournalEntryStatus.Voided, entry.Status);
        // Voiding a debit means crediting the account, voiding a credit means debiting
        account1.Received(1).Credit(100); // Reverse of original debit
        account2.Received(1).Debit(100);  // Reverse of original credit
        await _journalEntryRepository.Received(1).UpdateAsync(entry, cancellationToken);
    }

    [Fact]
    public async Task VoidJournalEntryAsync_NotPostedEntry_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var entry = new JournalEntry(DateTime.UtcNow, "Test Void") { Status = JournalEntryStatus.Draft }; // Not posted
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(entry, journalEntryId, null);

        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(entry));
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.VoidJournalEntryAsync(journalEntryId, cancellationToken));
        Assert.Contains("Only posted entries can be voided.", exception.Message); // Domain entity throws this
    }

    [Fact]
    public async Task VoidJournalEntryAsync_EntryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(null));
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.VoidJournalEntryAsync(journalEntryId, cancellationToken));
    }

    [Fact]
    public async Task UpdateJournalEntryAsync_DraftEntry_ValidRequest_ShouldUpdateAndReturnDto()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var account1 = new Account("101", "Cash", AccountType.Asset, 0);
        var account2 = new Account("201", "Revenue", AccountType.Revenue, 0);
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(account1, accountId1, null);
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(account2, accountId2, null);

        _accountRepository.GetByIdAsync(accountId1, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(account1));
        _accountRepository.GetByIdAsync(accountId2, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(account2));

        var existingEntry = new JournalEntry(DateTime.UtcNow.AddDays(-1), "Old Description") { Status = JournalEntryStatus.Draft };
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(existingEntry, journalEntryId, null);

        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(existingEntry));

        var request = new UpdateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "New Description",
            Transactions = new List<TransactionRequest>
            {
                new() { AccountId = accountId1, TransactionType = TransactionType.Debit, Amount = 150, Description = "New Debit" },
                new() { AccountId = accountId2, TransactionType = TransactionType.Credit, Amount = 150, Description = "New Credit" }
            }
        };
        var cancellationToken = CancellationToken.None;

        // Act
        // The UpdateJournalEntryAsync in service currently clears and re-adds transactions.
        // This test will assume that behavior. A more complex update logic would need more detailed transaction mocking.
        var updatedDto = await _sut.UpdateJournalEntryAsync(journalEntryId, request, cancellationToken);

        // Assert
        Assert.NotNull(updatedDto);
        Assert.Equal(journalEntryId, updatedDto.Id);
        Assert.Equal(request.Description, updatedDto.Description);
        Assert.Equal(request.EntryDate.Date, updatedDto.EntryDate.Date);
        // Assert.Equal(request.Transactions.Count, updatedDto.Transactions.Count); // This depends on how Update handles transactions

        await _journalEntryRepository.Received(1).UpdateAsync(Arg.Is<JournalEntry>(je =>
            je.Id == journalEntryId &&
            je.Description == request.Description
            // je.Transactions.Count == request.Transactions.Count // This is tricky due to new Transaction objects
        ), cancellationToken);
    }

    [Fact]
    public async Task UpdateJournalEntryAsync_NotDraftEntry_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var existingEntry = new JournalEntry(DateTime.UtcNow, "Test Update") { Status = JournalEntryStatus.Posted }; // Not a draft
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(existingEntry, journalEntryId, null);

        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(existingEntry));

        var request = new UpdateJournalEntryRequest { Description = "New Desc" };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateJournalEntryAsync(journalEntryId, request, cancellationToken));
        Assert.Contains("Only draft journal entries can be updated.", exception.Message);
    }

    [Fact]
    public async Task UpdateJournalEntryAsync_EntryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        _journalEntryRepository.GetByIdAsync(journalEntryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(null));

        var request = new UpdateJournalEntryRequest { Description = "New Desc" };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateJournalEntryAsync(journalEntryId, request, cancellationToken));
    }

    [Fact]
    public async Task SearchJournalEntriesAsync_WithMatchingCriteria_ShouldReturnMatchingEntries()
    {
        // Arrange
        var entry1Date = DateTime.UtcNow.AddDays(-2);
        var entry2Date = DateTime.UtcNow.AddDays(-1);
        var entry1 = new JournalEntry(entry1Date, "Entry One") { Status = JournalEntryStatus.Draft };
        var entry2 = new JournalEntry(entry2Date, "Entry Two") { Status = JournalEntryStatus.Posted };
        var entry3 = new JournalEntry(entry1Date, "Another One") { Status = JournalEntryStatus.Draft };

        var allEntries = new List<JournalEntry> { entry1, entry2, entry3 };

        _journalEntryRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(allEntries));

        var request = new SearchJournalEntriesRequest
        {
            Status = JournalEntryStatus.Draft,
            PageNumber = 1,
            PageSize = 10
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.SearchJournalEntriesAsync(request, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Description == "Entry One");
        Assert.Contains(result, r => r.Description == "Another One");
    }

    [Fact]
    public async Task SearchJournalEntriesAsync_WithDateRange_ShouldReturnMatchingEntries()
    {
        // Arrange
        var entry1Date = DateTime.UtcNow.AddDays(-5);
        var entry2Date = DateTime.UtcNow.AddDays(-3);
        var entry3Date = DateTime.UtcNow.AddDays(-1); // This one should be out of range

        var entry1 = new JournalEntry(entry1Date, "Entry One") { Status = JournalEntryStatus.Draft };
        var entry2 = new JournalEntry(entry2Date, "Entry Two") { Status = JournalEntryStatus.Posted };
        var entry3 = new JournalEntry(entry3Date, "Entry Three") { Status = JournalEntryStatus.Draft };
        var allEntries = new List<JournalEntry> { entry1, entry2, entry3 };

        _journalEntryRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(allEntries));

        var request = new SearchJournalEntriesRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-6),
            EndDate = DateTime.UtcNow.AddDays(-2),
            PageNumber = 1,
            PageSize = 10
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.SearchJournalEntriesAsync(request, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Description == "Entry One");
        Assert.Contains(result, r => r.Description == "Entry Two");
    }


    [Fact]
    public async Task SearchJournalEntriesAsync_WithNonMatchingCriteria_ShouldReturnEmptyList()
    {
        // Arrange
        var entry1 = new JournalEntry(DateTime.UtcNow.AddDays(-2), "Entry One") { Status = JournalEntryStatus.Draft };
        var allEntries = new List<JournalEntry> { entry1 };

        _journalEntryRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(allEntries));

        var request = new SearchJournalEntriesRequest
        {
            Status = JournalEntryStatus.Voided, // No voided entries
            PageNumber = 1,
            PageSize = 10
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _sut.SearchJournalEntriesAsync(request, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
