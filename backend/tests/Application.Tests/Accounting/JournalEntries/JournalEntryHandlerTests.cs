using FluentAssertions;
using FSH.WebApi.Application.Accounting.JournalEntries;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Common.Contracts; // For IAggregateRoot
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Mapster;
using Xunit;
using FluentValidation;

namespace FSH.WebApi.Application.Tests.Accounting.JournalEntries;

public class JournalEntryHandlerTests
{
    private readonly IRepository<JournalEntry> _journalEntryRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IStringLocalizer<CreateJournalEntryHandler> _createLocalizer;
    private readonly ILogger<CreateJournalEntryHandler> _createLogger;
    private readonly IStringLocalizer<PostJournalEntryHandler> _postLocalizer;
    private readonly ILogger<PostJournalEntryHandler> _postLogger;
    private readonly IStringLocalizer<GetJournalEntryHandler> _getLocalizer;
    private readonly IStringLocalizer<SearchJournalEntriesHandler> _searchLocalizer;

    public JournalEntryHandlerTests()
    {
        _journalEntryRepository = Substitute.For<IRepository<JournalEntry>>();
        _accountRepository = Substitute.For<IRepository<Account>>();

        _createLocalizer = Substitute.For<IStringLocalizer<CreateJournalEntryHandler>>();
        _createLogger = Substitute.For<ILogger<CreateJournalEntryHandler>>();
        _postLocalizer = Substitute.For<IStringLocalizer<PostJournalEntryHandler>>();
        _postLogger = Substitute.For<ILogger<PostJournalEntryHandler>>();
        _getLocalizer = Substitute.For<IStringLocalizer<GetJournalEntryHandler>>();
        _searchLocalizer = Substitute.For<IStringLocalizer<SearchJournalEntriesHandler>>();

        // Setup default localization messages for exceptions if needed
        _createLocalizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        _createLocalizer[Arg.Any<string>(), Arg.Any<object[]>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        _postLocalizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        _postLocalizer[Arg.Any<string>(), Arg.Any<object[]>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        _getLocalizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        _getLocalizer[Arg.Any<string>(), Arg.Any<object[]>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
    }

    // === CreateJournalEntryHandler Tests ===
    [Fact]
    public async Task CreateJournalEntryHandler_Should_Create_And_Return_JournalEntryId_When_Request_Is_Valid()
    {
        // Arrange
        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Valid JE",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = "Debit", Amount = 100 },
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = "Credit", Amount = 100 }
            }
        };

        foreach (var item in request.Transactions)
        {
            _accountRepository.GetByIdAsync(item.AccountId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Account?>(new Account("Test Acc", "123", AccountType.Asset, 0, null, true)));
        }

        JournalEntry? capturedJournalEntry = null;
        await _journalEntryRepository.AddAsync(Arg.Do<JournalEntry>(je => capturedJournalEntry = je), Arg.Any<CancellationToken>());

        var handler = new CreateJournalEntryHandler(_journalEntryRepository, _accountRepository, _createLocalizer, _createLogger);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _journalEntryRepository.Received(1).AddAsync(Arg.Any<JournalEntry>(), Arg.Any<CancellationToken>());
        capturedJournalEntry.Should().NotBeNull();
        capturedJournalEntry!.Id.Should().Be(result);
        capturedJournalEntry.Description.Should().Be(request.Description);
        capturedJournalEntry.Transactions.Should().HaveCount(request.Transactions.Count);
    }

    [Fact]
    public async Task CreateJournalEntryHandler_Should_Throw_NotFoundException_When_Transaction_AccountId_Not_Found()
    {
        // Arrange
        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "JE with invalid account",
            Transactions = new List<CreateTransactionRequestItem> { new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = "Debit", Amount = 100 } }
        };

        _accountRepository.GetByIdAsync(request.Transactions[0].AccountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(null)); // Account not found

        var handler = new CreateJournalEntryHandler(_journalEntryRepository, _accountRepository, _createLocalizer, _createLogger);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateJournalEntryHandler_Should_Throw_ValidationException_When_Account_Is_Not_Active()
    {
        // Arrange
        var inactiveAccountId = Guid.NewGuid();
        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "JE with inactive account",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = inactiveAccountId, TransactionType = "Debit", Amount = 50 },
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = "Credit", Amount = 50 }
            }
        };

        _accountRepository.GetByIdAsync(inactiveAccountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(new Account("Inactive", "001", AccountType.Asset, 0m, null, false)));
        _accountRepository.GetByIdAsync(Arg.Is<Guid>(id => id != inactiveAccountId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(new Account("Active", "002", AccountType.Asset, 0m, null, true)));

        var handler = new CreateJournalEntryHandler(_journalEntryRepository, _accountRepository, _createLocalizer, _createLogger);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === PostJournalEntryHandler Tests ===
    [Fact]
    public async Task PostJournalEntryHandler_Should_Post_JournalEntry_And_Update_Account_Balances()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var assetAccountId = Guid.NewGuid();
        var liabilityAccountId = Guid.NewGuid();

        var journalEntry = new JournalEntry(DateTime.UtcNow.AddDays(-1), "Test JE to Post", null);
        typeof(AuditableEntity).GetProperty(nameof(AuditableEntity.Id))!.SetValue(journalEntry, journalEntryId); // Set Id

        var transactions = new List<Transaction>
        {
            new Transaction(journalEntryId, assetAccountId, TransactionType.Debit, 100m, "Asset Debit"),
            new Transaction(journalEntryId, liabilityAccountId, TransactionType.Credit, 100m, "Liability Credit")
        };
        journalEntry.Transactions = transactions; // Manually set as AddTransaction is instance method

        var assetAccount = new Account("Cash", "1000", AccountType.Asset, 50m, "Cash account", true); // Initial Balance 50
        typeof(AuditableEntity).GetProperty(nameof(AuditableEntity.Id))!.SetValue(assetAccount, assetAccountId);

        var liabilityAccount = new Account("Payables", "2000", AccountType.Liability, 20m, "Payables", true); // Initial Balance 20
        typeof(AuditableEntity).GetProperty(nameof(AuditableEntity.Id))!.SetValue(liabilityAccount, liabilityAccountId);


        _journalEntryRepository.FirstOrDefaultAsync(Arg.Any<JournalEntryWithTransactionsSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(journalEntry));
        _accountRepository.GetByIdAsync(assetAccountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(assetAccount));
        _accountRepository.GetByIdAsync(liabilityAccountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(liabilityAccount));

        var handler = new PostJournalEntryHandler(_journalEntryRepository, _accountRepository, _postLocalizer, _postLogger);
        var request = new PostJournalEntryRequest(journalEntryId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(journalEntryId);
        journalEntry.IsPosted.Should().BeTrue();
        journalEntry.PostedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify account balance updates
        // Asset (Debit increases): 50 + 100 = 150
        // Liability (Credit increases): 20 + 100 = 120
        await _accountRepository.Received(1).UpdateAsync(Arg.Is<Account>(a => a.Id == assetAccountId && a.Balance == 150m), Arg.Any<CancellationToken>());
        await _accountRepository.Received(1).UpdateAsync(Arg.Is<Account>(a => a.Id == liabilityAccountId && a.Balance == 120m), Arg.Any<CancellationToken>());
        await _journalEntryRepository.Received(1).UpdateAsync(journalEntry, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PostJournalEntryHandler_Should_Throw_NotFoundException_When_JournalEntry_Not_Found()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        _journalEntryRepository.FirstOrDefaultAsync(Arg.Any<JournalEntryWithTransactionsSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(null));
        var handler = new PostJournalEntryHandler(_journalEntryRepository, _accountRepository, _postLocalizer, _postLogger);
        var request = new PostJournalEntryRequest(journalEntryId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task PostJournalEntryHandler_Should_Throw_ValidationException_When_JournalEntry_Is_Already_Posted()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var journalEntry = new JournalEntry(DateTime.UtcNow, "Already Posted JE", null);
        typeof(AuditableEntity).GetProperty(nameof(AuditableEntity.Id))!.SetValue(journalEntry, journalEntryId);
        journalEntry.Post(); // Mark as posted

        _journalEntryRepository.FirstOrDefaultAsync(Arg.Any<JournalEntryWithTransactionsSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(journalEntry));
        var handler = new PostJournalEntryHandler(_journalEntryRepository, _accountRepository, _postLocalizer, _postLogger);
        var request = new PostJournalEntryRequest(journalEntryId);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task PostJournalEntryHandler_Should_Throw_ValidationException_When_JournalEntry_Has_No_Transactions()
    {
        var journalEntryId = Guid.NewGuid();
        var journalEntry = new JournalEntry(DateTime.UtcNow, "No Tx JE", null);
        typeof(AuditableEntity).GetProperty(nameof(AuditableEntity.Id))!.SetValue(journalEntry, journalEntryId);

        _journalEntryRepository.FirstOrDefaultAsync(Arg.Any<JournalEntryWithTransactionsSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(journalEntry));

        var handler = new PostJournalEntryHandler(_journalEntryRepository, _accountRepository, _postLocalizer, _postLogger);
        var request = new PostJournalEntryRequest(journalEntryId);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task PostJournalEntryHandler_Should_Throw_InvalidOperationException_When_Debits_Do_Not_Equal_Credits()
    {
        var journalEntryId = Guid.NewGuid();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();

        var journalEntry = new JournalEntry(DateTime.UtcNow, "Unbalanced JE", null);
        typeof(AuditableEntity).GetProperty(nameof(AuditableEntity.Id))!.SetValue(journalEntry, journalEntryId);
        journalEntry.Transactions = new List<Transaction>
        {
            new Transaction(journalEntryId, accountId1, TransactionType.Debit, 100m, "Debit"),
            new Transaction(journalEntryId, accountId2, TransactionType.Credit, 50m, "Credit")
        };

        _journalEntryRepository.FirstOrDefaultAsync(Arg.Any<JournalEntryWithTransactionsSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(journalEntry));
        _accountRepository.GetByIdAsync(accountId1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(new Account("A1", "1", AccountType.Asset, 0m, null, true)));
        _accountRepository.GetByIdAsync(accountId2, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(new Account("A2", "2", AccountType.Liability, 0m, null, true)));

        var handler = new PostJournalEntryHandler(_journalEntryRepository, _accountRepository, _postLocalizer, _postLogger);
        var request = new PostJournalEntryRequest(journalEntryId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task PostJournalEntryHandler_Should_Throw_NotFoundException_When_Account_Not_Found()
    {
        var journalEntryId = Guid.NewGuid();
        var missingAccountId = Guid.NewGuid();

        var journalEntry = new JournalEntry(DateTime.UtcNow, "JE", null);
        typeof(AuditableEntity).GetProperty(nameof(AuditableEntity.Id))!.SetValue(journalEntry, journalEntryId);
        journalEntry.Transactions = new List<Transaction>
        {
            new Transaction(journalEntryId, missingAccountId, TransactionType.Debit, 50m, "Debit")
        };

        _journalEntryRepository.FirstOrDefaultAsync(Arg.Any<JournalEntryWithTransactionsSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(journalEntry));
        _accountRepository.GetByIdAsync(missingAccountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(null));

        var handler = new PostJournalEntryHandler(_journalEntryRepository, _accountRepository, _postLocalizer, _postLogger);
        var request = new PostJournalEntryRequest(journalEntryId);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task PostJournalEntryHandler_Should_Throw_InvalidOperationException_When_AccountType_Unknown()
    {
        var journalEntryId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var journalEntry = new JournalEntry(DateTime.UtcNow, "JE", null);
        typeof(AuditableEntity).GetProperty(nameof(AuditableEntity.Id))!.SetValue(journalEntry, journalEntryId);
        journalEntry.Transactions = new List<Transaction>
        {
            new Transaction(journalEntryId, accountId, TransactionType.Debit, 10m, "Debit")
        };

        _journalEntryRepository.FirstOrDefaultAsync(Arg.Any<JournalEntryWithTransactionsSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(journalEntry));

        var invalidAccount = new Account("Weird", "W001", (AccountType)999, 0m, null);
        typeof(AuditableEntity).GetProperty(nameof(AuditableEntity.Id))!.SetValue(invalidAccount, accountId);
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(invalidAccount));

        var handler = new PostJournalEntryHandler(_journalEntryRepository, _accountRepository, _postLocalizer, _postLogger);
        var request = new PostJournalEntryRequest(journalEntryId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === GetJournalEntryHandler Tests ===
    [Fact]
    public async Task GetJournalEntryHandler_Should_Return_JournalEntryDto_When_Exists()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();

        var journalEntry = new JournalEntry(DateTime.UtcNow, "Test JE", "REF001");
        typeof(AuditableEntity).GetProperty(nameof(AuditableEntity.Id))!.SetValue(journalEntry, journalEntryId);
        journalEntry.Transactions = new List<Transaction>
        {
            new Transaction(journalEntryId, accountId1, TransactionType.Debit, 100m, "Debit Desc") { Id = Guid.NewGuid() },
            new Transaction(journalEntryId, accountId2, TransactionType.Credit, 100m, "Credit Desc") { Id = Guid.NewGuid() }
        };

        _journalEntryRepository.FirstOrDefaultAsync(Arg.Any<JournalEntryWithTransactionsSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(journalEntry));
        _accountRepository.GetByIdAsync(accountId1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(new Account("Acc1", "A001", AccountType.Asset, 0, null)));
        _accountRepository.GetByIdAsync(accountId2, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Account?>(new Account("Acc2", "A002", AccountType.Liability, 0, null)));

        var handler = new GetJournalEntryHandler(_journalEntryRepository, _getLocalizer, _accountRepository);
        var request = new GetJournalEntryRequest(journalEntryId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(journalEntryId);
        result.Description.Should().Be("Test JE");
        result.Transactions.Should().HaveCount(2);
        result.Transactions[0].AccountName.Should().Be("Acc1");
        result.Transactions[0].TransactionType.Should().Be(TransactionType.Debit.ToString());
    }

    [Fact]
    public async Task GetJournalEntryHandler_Should_Throw_NotFoundException_When_Not_Exists()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        _journalEntryRepository.FirstOrDefaultAsync(Arg.Any<JournalEntryWithTransactionsSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JournalEntry?>(null));
        var handler = new GetJournalEntryHandler(_journalEntryRepository, _getLocalizer, _accountRepository);
        var request = new GetJournalEntryRequest(journalEntryId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === SearchJournalEntriesHandler Tests ===
    [Fact]
    public async Task SearchJournalEntriesHandler_Should_Return_PaginationResponse_Of_JournalEntryDto()
    {
        // Arrange
        var request = new SearchJournalEntriesRequest { PageNumber = 1, PageSize = 10 };
        var journalEntriesList = new List<JournalEntry>
        {
            new JournalEntry(DateTime.UtcNow, "JE1", null) { Id = Guid.NewGuid(), Transactions = new List<Transaction>() },
            new JournalEntry(DateTime.UtcNow, "JE2", null) { Id = Guid.NewGuid(), Transactions = new List<Transaction>() }
        };

        // Basic mocking for ListAsync and CountAsync
        _journalEntryRepository.ListAsync(Arg.Any<ISpecification<JournalEntry, JournalEntryDto>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(journalEntriesList.Adapt<List<JournalEntryDto>>()));
        _journalEntryRepository.CountAsync(Arg.Any<ISpecification<JournalEntry, JournalEntryDto>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(journalEntriesList.Count));

        var handler = new SearchJournalEntriesHandler(_journalEntryRepository, _searchLocalizer, _accountRepository);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(journalEntriesList.Count);
        result.TotalCount.Should().Be(journalEntriesList.Count);
        result.Data[0].Description.Should().Be("JE1");
    }
}
