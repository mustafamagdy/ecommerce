using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.BankAccounts;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Application.Common.Persistence;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Models;
using Ardalis.Specification;

namespace Application.Tests.Accounting.BankAccounts;

public class BankAccountHandlerTests
{
    private readonly Mock<IRepository<BankAccount>> _mockBankAccountRepo;
    private readonly Mock<IReadRepository<BankAccount>> _mockBankAccountReadRepo;
    private readonly Mock<IReadRepository<Account>> _mockGlAccountReadRepo;
    private readonly Mock<IReadRepository<BankStatement>> _mockStatementReadRepo;
    private readonly Mock<IReadRepository<BankReconciliation>> _mockReconciliationReadRepo;

    private readonly Mock<IStringLocalizer<CreateBankAccountHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateBankAccountHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<UpdateBankAccountHandler>> _mockUpdateLocalizer;
    private readonly Mock<ILogger<UpdateBankAccountHandler>> _mockUpdateLogger;
    private readonly Mock<IStringLocalizer<GetBankAccountHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<DeleteBankAccountHandler>> _mockDeleteLocalizer;
    private readonly Mock<ILogger<DeleteBankAccountHandler>> _mockDeleteLogger;
    // SearchBankAccountsHandler doesn't have localizer/logger in its constructor from previous implementation.

    public BankAccountHandlerTests()
    {
        _mockBankAccountRepo = new Mock<IRepository<BankAccount>>();
        _mockBankAccountReadRepo = new Mock<IReadRepository<BankAccount>>();
        _mockGlAccountReadRepo = new Mock<IReadRepository<Account>>();
        _mockStatementReadRepo = new Mock<IReadRepository<BankStatement>>();
        _mockReconciliationReadRepo = new Mock<IReadRepository<BankReconciliation>>();

        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateBankAccountHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateBankAccountHandler>>();
        _mockUpdateLocalizer = new Mock<IStringLocalizer<UpdateBankAccountHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateBankAccountHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetBankAccountHandler>>();
        _mockDeleteLocalizer = new Mock<IStringLocalizer<DeleteBankAccountHandler>>();
        _mockDeleteLogger = new Mock<ILogger<DeleteBankAccountHandler>>();

        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockUpdateLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
        SetupDefaultLocalizationMock(_mockDeleteLocalizer);
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class =>
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] args) => new LocalizedString(name, name));

    private BankAccount CreateSampleBankAccount(Guid id, string accNumber, Guid glAccountId) =>
        new BankAccount("Sample Bank Acc", accNumber, "Bank Inc", "USD", glAccountId) { Id = id };
    private Account CreateSampleGlAccount(Guid id, string accNumber = "10100", string name = "Cash Main") =>
        new Account(name, accNumber, AccountType.Asset, 0, "GL Account desc", true) { Id = id };

    // === CreateBankAccountHandler Tests ===
    [Fact]
    public async Task CreateBankAccountHandler_Should_CreateAccount_WhenValid()
    {
        var glAccountId = Guid.NewGuid();
        var request = new CreateBankAccountRequest { AccountName = "New Bank", AccountNumber = "NEWACC001", BankName = "NBank", Currency = "USD", GLAccountId = glAccountId, IsActive = true };
        _mockGlAccountReadRepo.Setup(r => r.GetByIdAsync(glAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleGlAccount(glAccountId));
        _mockBankAccountRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<BankAccountByAccountNumberSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync((BankAccount)null!);
        _mockBankAccountRepo.Setup(r => r.AddAsync(It.IsAny<BankAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BankAccount ba, CancellationToken ct) => { ba.Id = Guid.NewGuid(); return ba; });
        var handler = new CreateBankAccountHandler(_mockBankAccountRepo.Object, _mockGlAccountReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeEmpty();
        _mockBankAccountRepo.Verify(r => r.AddAsync(It.Is<BankAccount>(ba => ba.AccountNumber == request.AccountNumber), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBankAccountHandler_Should_ThrowConflict_WhenAccountNumberExists()
    {
        var request = new CreateBankAccountRequest { AccountNumber = "DUPACC001", GLAccountId = Guid.NewGuid() };
        _mockGlAccountReadRepo.Setup(r => r.GetByIdAsync(request.GLAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleGlAccount(request.GLAccountId));
        _mockBankAccountRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<BankAccountByAccountNumberSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSampleBankAccount(Guid.NewGuid(), request.AccountNumber, request.GLAccountId));
        var handler = new CreateBankAccountHandler(_mockBankAccountRepo.Object, _mockGlAccountReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateBankAccountHandler_Should_ThrowNotFound_WhenGLAccountNotFound()
    {
        var request = new CreateBankAccountRequest { GLAccountId = Guid.NewGuid(), AccountNumber = "ANYACC" };
        _mockGlAccountReadRepo.Setup(r => r.GetByIdAsync(request.GLAccountId, It.IsAny<CancellationToken>())).ReturnsAsync((Account)null!);
        var handler = new CreateBankAccountHandler(_mockBankAccountRepo.Object, _mockGlAccountReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === UpdateBankAccountHandler Tests ===
    [Fact]
    public async Task UpdateBankAccountHandler_Should_UpdateAccount_WhenValid()
    {
        var accountId = Guid.NewGuid();
        var glAccountId = Guid.NewGuid();
        var request = new UpdateBankAccountRequest { Id = accountId, AccountName = "Updated Name", GLAccountId = glAccountId };
        var existingAccount = CreateSampleBankAccount(accountId, "OLDACC", Guid.NewGuid()); // Different GL initially
        _mockBankAccountRepo.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(existingAccount);
        _mockGlAccountReadRepo.Setup(r => r.GetByIdAsync(glAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleGlAccount(glAccountId)); // Valid new GL
        var handler = new UpdateBankAccountHandler(_mockBankAccountRepo.Object, _mockGlAccountReadRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Be(accountId);
        _mockBankAccountRepo.Verify(r => r.UpdateAsync(It.Is<BankAccount>(ba => ba.AccountName == request.AccountName && ba.GLAccountId == glAccountId), It.IsAny<CancellationToken>()), Times.Once);
        existingAccount.AccountName.Should().Be(request.AccountName); // Check if Update method on entity was effective
        existingAccount.GLAccountId.Should().Be(glAccountId);
    }

    // === GetBankAccountHandler Tests ===
    [Fact]
    public async Task GetBankAccountHandler_Should_ReturnDtoWithGLDetails_WhenFound()
    {
        var accountId = Guid.NewGuid();
        var glAccountId = Guid.NewGuid();
        var request = new GetBankAccountRequest(accountId);
        var bankAccountEntity = CreateSampleBankAccount(accountId, "GETACC001", glAccountId);
        var glAccountEntity = CreateSampleGlAccount(glAccountId, "10101", "Cash Main Branch");

        // The handler first gets BankAccount, then gets GLAccount.
        // BankAccountByIdSpec is Specification<BankAccount, BankAccountDto>
        // The current GetBankAccountHandler logic fetches entity, then DTO adapts, then populates GL.
        // If spec returns DTO: _mockBankAccountReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<BankAccountByIdSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(bankAccountEntity.Adapt<BankAccountDto>());
        // If spec returns entity:
        _mockBankAccountReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<BankAccountByIdSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankAccountEntity.Adapt<BankAccountDto>()); // Assuming the spec returns DTO
                                                                      // and handler logic for null DTO then fetches entity.
                                                                      // Let's align with the handler code which expects DTO from spec.
        _mockGlAccountReadRepo.Setup(r => r.GetByIdAsync(glAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(glAccountEntity);

        var handler = new GetBankAccountHandler(_mockBankAccountReadRepo.Object, _mockGlAccountReadRepo.Object, _mockGetLocalizer.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(accountId);
        result.GLAccountCode.Should().Be(glAccountEntity.AccountNumber);
        result.GLAccountName.Should().Be(glAccountEntity.AccountName);
    }


    // === SearchBankAccountsHandler Tests ===
    [Fact]
    public async Task SearchBankAccountsHandler_Should_ReturnPaginatedDtosWithGLDetails()
    {
        var request = new SearchBankAccountsRequest { PageNumber = 1, PageSize = 10 };
        var glAccountId1 = Guid.NewGuid();
        var bankAccountList = new List<BankAccountDto> // Assuming spec returns DTOs
        {
            CreateSampleBankAccount(Guid.NewGuid(), "SEARCH001", glAccountId1).Adapt<BankAccountDto>()
        };
        var glAccount1 = CreateSampleGlAccount(glAccountId1, "10200", "Search GL Cash");

        _mockBankAccountReadRepo.Setup(r => r.ListAsync(It.IsAny<BankAccountsBySearchFilterSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(bankAccountList);
        _mockBankAccountReadRepo.Setup(r => r.CountAsync(It.IsAny<BankAccountsBySearchFilterSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(bankAccountList.Count);
        _mockGlAccountReadRepo.Setup(r => r.ListAsync(It.Is<GLAccountsByIdsSpec>(s => s.Ids.Contains(glAccountId1)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account> { glAccount1 });

        var handler = new SearchBankAccountsHandler(_mockBankAccountReadRepo.Object, _mockGlAccountReadRepo.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(bankAccountList.Count);
        result.Data.First().GLAccountCode.Should().Be(glAccount1.AccountNumber);
        result.Data.First().GLAccountName.Should().Be(glAccount1.AccountName);
    }

    // === DeleteBankAccountHandler Tests ===
    [Fact]
    public async Task DeleteBankAccountHandler_Should_DeleteAccount_WhenNoDependencies()
    {
        var accountId = Guid.NewGuid();
        var request = new DeleteBankAccountRequest(accountId);
        var bankAccountEntity = CreateSampleBankAccount(accountId, "DELACC001", Guid.NewGuid());
        _mockBankAccountRepo.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(bankAccountEntity);
        _mockStatementReadRepo.Setup(r => r.AnyAsync(It.IsAny<BankStatementsByBankAccountIdSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockReconciliationReadRepo.Setup(r => r.AnyAsync(It.IsAny<BankReconciliationsByBankAccountIdSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new DeleteBankAccountHandler(_mockBankAccountRepo.Object, _mockStatementReadRepo.Object, _mockReconciliationReadRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Be(accountId);
        _mockBankAccountRepo.Verify(r => r.DeleteAsync(bankAccountEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBankAccountHandler_Should_ThrowConflict_WhenStatementsExist()
    {
        var accountId = Guid.NewGuid();
        var request = new DeleteBankAccountRequest(accountId);
        var bankAccountEntity = CreateSampleBankAccount(accountId, "DELACC002", Guid.NewGuid());
        _mockBankAccountRepo.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(bankAccountEntity);
        _mockStatementReadRepo.Setup(r => r.AnyAsync(It.IsAny<BankStatementsByBankAccountIdSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(true); // Statements exist
        var handler = new DeleteBankAccountHandler(_mockBankAccountRepo.Object, _mockStatementReadRepo.Object, _mockReconciliationReadRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }
}
