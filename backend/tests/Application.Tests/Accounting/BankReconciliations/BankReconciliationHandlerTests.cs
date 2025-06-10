using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.BankReconciliations;
using FSH.WebApi.Application.Accounting.BankStatements; // For BankStatementTransactionByIdSpec and DTO
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Application.Common.Persistence;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Models;
using Ardalis.Specification;

namespace Application.Tests.Accounting.BankReconciliations;

public class BankReconciliationHandlerTests
{
    private readonly Mock<IRepository<BankReconciliation>> _mockReconRepo;
    private readonly Mock<IReadRepository<BankReconciliation>> _mockReconReadRepo;
    private readonly Mock<IReadRepository<BankAccount>> _mockBankAccountReadRepo;
    private readonly Mock<IReadRepository<BankStatement>> _mockBankStatementReadRepo;
    private readonly Mock<IRepository<BankStatementTransaction>> _mockStatementTransactionRepo; // Writable for Match handler
    private readonly Mock<IReadRepository<BankStatementTransaction>> _mockStatementTransactionReadRepo; // For GetBankStatementTransactions...
    private readonly Mock<IReadRepository<Account>> _mockGlAccountReadRepo;

    private readonly Mock<IStringLocalizer<CreateBankReconciliationHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateBankReconciliationHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<UpdateBankReconciliationHandler>> _mockUpdateLocalizer;
    private readonly Mock<ILogger<UpdateBankReconciliationHandler>> _mockUpdateLogger;
    private readonly Mock<IStringLocalizer<MatchBankTransactionHandler>> _mockMatchLocalizer;
    private readonly Mock<ILogger<MatchBankTransactionHandler>> _mockMatchLogger;
    private readonly Mock<IStringLocalizer<GetBankReconciliationHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<SearchBankReconciliationsHandler>> _mockSearchLocalizer;
    private readonly Mock<IStringLocalizer<GetBankStatementTransactionsForReconciliationHandler>> _mockGetTransactionsLocalizer;


    public BankReconciliationHandlerTests()
    {
        _mockReconRepo = new Mock<IRepository<BankReconciliation>>();
        _mockReconReadRepo = new Mock<IReadRepository<BankReconciliation>>();
        _mockBankAccountReadRepo = new Mock<IReadRepository<BankAccount>>();
        _mockBankStatementReadRepo = new Mock<IReadRepository<BankStatement>>();
        _mockStatementTransactionRepo = new Mock<IRepository<BankStatementTransaction>>();
        _mockStatementTransactionReadRepo = new Mock<IReadRepository<BankStatementTransaction>>();
        _mockGlAccountReadRepo = new Mock<IReadRepository<Account>>();

        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateBankReconciliationHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateBankReconciliationHandler>>();
        _mockUpdateLocalizer = new Mock<IStringLocalizer<UpdateBankReconciliationHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateBankReconciliationHandler>>();
        _mockMatchLocalizer = new Mock<IStringLocalizer<MatchBankTransactionHandler>>();
        _mockMatchLogger = new Mock<ILogger<MatchBankTransactionHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetBankReconciliationHandler>>();
        _mockSearchLocalizer = new Mock<IStringLocalizer<SearchBankReconciliationsHandler>>(); // Not used by handler
        _mockGetTransactionsLocalizer = new Mock<IStringLocalizer<GetBankStatementTransactionsForReconciliationHandler>>();


        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockUpdateLocalizer);
        SetupDefaultLocalizationMock(_mockMatchLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
        // SetupDefaultLocalizationMock(_mockSearchLocalizer); // Not used
        SetupDefaultLocalizationMock(_mockGetTransactionsLocalizer);
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class =>
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] args) => new LocalizedString(name, name));

    // Helpers
    private BankAccount CreateSampleBankAccount(Guid id, Guid glAccountId, string name = "Test Bank Account") =>
        new BankAccount(name, "ACC123", "Bank Corp", "USD", glAccountId) { Id = id };
    private Account CreateSampleGlAccount(Guid id, decimal balance = 1000m) =>
        new Account("Cash Main", "10100", AccountType.Asset, balance, "GL Cash Account", true) { Id = id };
    private BankStatement CreateSampleBankStatement(Guid id, Guid bankAccountId, decimal closingBalance = 1200m, string refNum = "STMT001") =>
        new BankStatement(bankAccountId, DateTime.UtcNow.Date, 500m, closingBalance, refNum) { Id = id };
    private BankReconciliation CreateSampleBankReconciliation(Guid id, Guid bankAccountId, Guid statementId, decimal stmtBalance, decimal sysBalance, ReconciliationStatus status = ReconciliationStatus.Draft)
    {
        var recon = new BankReconciliation(bankAccountId, DateTime.UtcNow.Date, statementId, stmtBalance, sysBalance, status);
        recon.Id = id;
        return recon;
    }
    private BankStatementTransaction CreateSampleBankStatementTransaction(Guid id, Guid statementId, decimal amount = 50m, BankTransactionType type = BankTransactionType.Debit)
    {
        var tx = new BankStatementTransaction(statementId, DateTime.UtcNow.Date, "Tx Desc", amount, type);
        tx.Id = id;
        return tx;
    }


    // === CreateBankReconciliationHandler Tests ===
    [Fact]
    public async Task CreateBankReconciliationHandler_Should_CreateReconciliation_WhenValid()
    {
        var bankAccountId = Guid.NewGuid();
        var glAccountId = Guid.NewGuid();
        var statementId = Guid.NewGuid();
        var request = new CreateBankReconciliationRequest { BankAccountId = bankAccountId, ReconciliationDate = DateTime.UtcNow.Date, BankStatementId = statementId };

        var bankAccount = CreateSampleBankAccount(bankAccountId, glAccountId);
        var bankStatement = CreateSampleBankStatement(statementId, bankAccountId, closingBalance: 1500m);
        var glAccount = CreateSampleGlAccount(glAccountId, balance: 1450m); // System balance

        _mockBankAccountReadRepo.Setup(r => r.GetByIdAsync(bankAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(bankAccount);
        _mockBankStatementReadRepo.Setup(r => r.GetByIdAsync(statementId, It.IsAny<CancellationToken>())).ReturnsAsync(bankStatement);
        _mockReconRepo.Setup(r => r.AnyAsync(It.IsAny<BankStatementReconciledSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(false); // Not reconciled
        _mockGlAccountReadRepo.Setup(r => r.GetByIdAsync(glAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(glAccount);
        _mockReconRepo.Setup(r => r.AddAsync(It.IsAny<BankReconciliation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BankReconciliation br, CancellationToken ct) => { br.Id = Guid.NewGuid(); return br; });

        var handler = new CreateBankReconciliationHandler(_mockReconRepo.Object, _mockBankAccountReadRepo.Object, _mockBankStatementReadRepo.Object, _mockGlAccountReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeEmpty();
        _mockReconRepo.Verify(r => r.AddAsync(It.Is<BankReconciliation>(br =>
            br.BankAccountId == bankAccountId &&
            br.BankStatementId == statementId &&
            br.StatementBalance == bankStatement.ClosingBalance &&
            br.SystemBalance == glAccount.Balance && // Verifying it took GL balance
            br.Status == ReconciliationStatus.Draft
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBankReconciliationHandler_Should_ThrowConflict_WhenStatementAlreadyReconciled()
    {
        var request = new CreateBankReconciliationRequest { BankAccountId = Guid.NewGuid(), BankStatementId = Guid.NewGuid() };
        _mockBankAccountReadRepo.Setup(r => r.GetByIdAsync(request.BankAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleBankAccount(request.BankAccountId, Guid.NewGuid()));
        _mockBankStatementReadRepo.Setup(r => r.GetByIdAsync(request.BankStatementId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleBankStatement(request.BankStatementId, request.BankAccountId));
        _mockReconRepo.Setup(r => r.AnyAsync(It.IsAny<BankStatementReconciledSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(true); // Already reconciled

        var handler = new CreateBankReconciliationHandler(_mockReconRepo.Object, _mockBankAccountReadRepo.Object, _mockBankStatementReadRepo.Object, _mockGlAccountReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === UpdateBankReconciliationHandler Tests ===
    [Fact]
    public async Task UpdateBankReconciliationHandler_Should_UpdateStatusAndAdjustments()
    {
        var reconId = Guid.NewGuid();
        var request = new UpdateBankReconciliationRequest { Id = reconId, Status = ReconciliationStatus.InProgress, ManuallyAssignedUnclearedChecks = 50m };
        var existingRecon = CreateSampleBankReconciliation(reconId, Guid.NewGuid(), Guid.NewGuid(), 1000, 1000, ReconciliationStatus.Draft);
        _mockReconRepo.Setup(r => r.GetByIdAsync(reconId, It.IsAny<CancellationToken>())).ReturnsAsync(existingRecon);
        var handler = new UpdateBankReconciliationHandler(_mockReconRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Be(reconId);
        _mockReconRepo.Verify(r => r.UpdateAsync(It.Is<BankReconciliation>(br => br.Status == request.Status && br.ManuallyAssignedUnclearedChecks == request.ManuallyAssignedUnclearedChecks), It.IsAny<CancellationToken>()), Times.Once);
        existingRecon.Status.Should().Be(request.Status);
        existingRecon.ManuallyAssignedUnclearedChecks.Should().Be(request.ManuallyAssignedUnclearedChecks);
    }

    // === MatchBankTransactionHandler Tests ===
    [Fact]
    public async Task MatchBankTransactionHandler_Should_MarkTransactionAsReconciled()
    {
        var reconId = Guid.NewGuid();
        var statementId = Guid.NewGuid(); // Statement linked to recon
        var transactionId = Guid.NewGuid();
        var systemTxId = Guid.NewGuid();
        var request = new MatchBankTransactionRequest { BankReconciliationId = reconId, BankStatementTransactionId = transactionId, IsMatched = true, SystemTransactionId = systemTxId, SystemTransactionType = "TestPayment" };

        var reconciliation = CreateSampleBankReconciliation(reconId, Guid.NewGuid(), statementId, 100, 100, ReconciliationStatus.InProgress);
        var bankStatement = CreateSampleBankStatement(statementId, reconciliation.BankAccountId); // Ensure statement belongs to recon's bank account
        var transaction = CreateSampleBankStatementTransaction(transactionId, statementId); // Ensure tx belongs to statement

        _mockReconRepo.Setup(r => r.GetByIdAsync(reconId, It.IsAny<CancellationToken>())).ReturnsAsync(reconciliation);
        _mockStatementTransactionRepo.Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
        _mockBankStatementReadRepo.Setup(r => r.GetByIdAsync(statementId, It.IsAny<CancellationToken>())).ReturnsAsync(bankStatement);

        var handler = new MatchBankTransactionHandler(_mockStatementTransactionRepo.Object, _mockReconRepo.Object, _mockBankStatementReadRepo.Object, _mockMatchLocalizer.Object, _mockMatchLogger.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Be(transactionId);
        transaction.IsReconciled.Should().BeTrue();
        transaction.BankReconciliationId.Should().Be(reconId);
        transaction.SystemTransactionId.Should().Be(systemTxId);
        _mockStatementTransactionRepo.Verify(r => r.UpdateAsync(transaction, It.IsAny<CancellationToken>()), Times.Once);
    }


    // === GetBankReconciliationHandler Tests ===
    [Fact]
    public async Task GetBankReconciliationHandler_Should_ReturnDtoWithDetails()
    {
        var reconId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var statementId = Guid.NewGuid();
        var request = new GetBankReconciliationRequest(reconId);

        var bankAccount = CreateSampleBankAccount(bankAccountId, Guid.NewGuid(), "My Bank");
        var bankStatement = CreateSampleBankStatement(statementId, bankAccountId, refNum: "STMT-GET");
        var reconEntity = CreateSampleBankReconciliation(reconId, bankAccountId, statementId, 1500, 1500);
        // Simulate Includes
        typeof(BankReconciliation).GetProperty("BankAccount")!.SetValue(reconEntity, bankAccount);
        typeof(BankReconciliation).GetProperty("BankStatement")!.SetValue(reconEntity, bankStatement);


        // Assuming spec returns DTO
        var reconDto = reconEntity.Adapt<BankReconciliationDto>();
        reconDto.BankAccountName = $"{bankAccount.BankName} - {bankAccount.AccountNumber}"; // Manual population for test
        reconDto.BankStatementReference = bankStatement.ReferenceNumber;


        _mockReconReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<BankReconciliationByIdWithDetailsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reconDto); // Handler expects DTO from spec

        var handler = new GetBankReconciliationHandler(_mockReconReadRepo.Object, _mockGetLocalizer.Object);
        var resultDto = await handler.Handle(request, CancellationToken.None);

        resultDto.Should().NotBeNull();
        resultDto.Id.Should().Be(reconId);
        resultDto.BankAccountName.Should().Be(reconDto.BankAccountName);
        resultDto.BankStatementReference.Should().Be(bankStatement.ReferenceNumber);
    }

    // === SearchBankReconciliationsHandler Tests ===
    [Fact]
    public async Task SearchBankReconciliationsHandler_Should_ReturnPaginatedDtos()
    {
        var request = new SearchBankReconciliationsRequest { PageNumber = 1, PageSize = 10 };
        var bankAccountId = Guid.NewGuid();
        var bankAccount = CreateSampleBankAccount(bankAccountId, Guid.NewGuid(), "Search Bank");
        var statementId = Guid.NewGuid();
        var bankStatement = CreateSampleBankStatement(statementId, bankAccountId, refNum: "STMT-Search");

        var reconListAsEntities = new List<BankReconciliation> { CreateSampleBankReconciliation(Guid.NewGuid(), bankAccountId, statementId, 100, 100) };
        // Simulate Includes
        typeof(BankReconciliation).GetProperty("BankAccount")!.SetValue(reconListAsEntities[0], bankAccount);
        typeof(BankReconciliation).GetProperty("BankStatement")!.SetValue(reconListAsEntities[0], bankStatement);

        var reconListAsDtos = reconListAsEntities.Adapt<List<BankReconciliationDto>>();
        // Manual DTO enrichment that would happen in handler or Mapster config for test
        reconListAsDtos[0].BankAccountName = $"{bankAccount.BankName} - {bankAccount.AccountNumber}";
        reconListAsDtos[0].BankStatementReference = bankStatement.ReferenceNumber;


        _mockReconReadRepo.Setup(r => r.ListAsync(It.IsAny<BankReconciliationsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reconListAsDtos); // Spec returns DTOs
        _mockReconReadRepo.Setup(r => r.CountAsync(It.IsAny<BankReconciliationsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reconListAsDtos.Count);

        var handler = new SearchBankReconciliationsHandler(_mockReconReadRepo.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(reconListAsDtos.Count);
        result.Data.First().BankAccountName.Should().Be(reconListAsDtos.First().BankAccountName);
    }


    // === GetBankStatementTransactionsForReconciliationHandler Tests ===
    [Fact]
    public async Task GetBankStatementTransactionsForReconciliationHandler_Should_ReturnTransactions()
    {
        var reconId = Guid.NewGuid();
        var statementId = Guid.NewGuid(); // Statement ID linked to the reconciliation
        var request = new GetBankStatementTransactionsForReconciliationRequest { BankReconciliationId = reconId, PageNumber = 1, PageSize = 10 };
        var reconciliation = CreateSampleBankReconciliation(reconId, Guid.NewGuid(), statementId, 100, 100);
        var transactionsList = new List<BankStatementTransactionDto> // Assuming spec returns DTOs
        {
            CreateSampleBankStatementTransaction(Guid.NewGuid(), statementId).Adapt<BankStatementTransactionDto>()
        };

        _mockReconReadRepo.Setup(r => r.GetByIdAsync(reconId, It.IsAny<CancellationToken>())).ReturnsAsync(reconciliation);
        // The spec BankStatementTransactionsForReconciliationSpec is Specification<BankStatementTransaction, BankStatementTransactionDto>
        _mockStatementTransactionReadRepo.Setup(r => r.ListAsync(It.IsAny<BankStatementTransactionsForReconciliationSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionsList);
        _mockStatementTransactionReadRepo.Setup(r => r.CountAsync(It.IsAny<BankStatementTransactionsForReconciliationSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionsList.Count);


        var handler = new GetBankStatementTransactionsForReconciliationHandler(_mockStatementTransactionReadRepo.Object, _mockReconReadRepo.Object, _mockGetTransactionsLocalizer.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(transactionsList.Count);
    }
}
