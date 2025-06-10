using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.BankStatements;
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

namespace Application.Tests.Accounting.BankStatements;

public class BankStatementHandlerTests
{
    private readonly Mock<IRepository<BankStatement>> _mockStatementRepo;
    private readonly Mock<IReadRepository<BankStatement>> _mockStatementReadRepo;
    private readonly Mock<IReadRepository<BankAccount>> _mockBankAccountReadRepo;

    private readonly Mock<IStringLocalizer<CreateBankStatementHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateBankStatementHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<GetBankStatementHandler>> _mockGetLocalizer;
    // SearchBankStatementsHandler constructor does not take IStringLocalizer or ILogger

    public BankStatementHandlerTests()
    {
        _mockStatementRepo = new Mock<IRepository<BankStatement>>();
        _mockStatementReadRepo = new Mock<IReadRepository<BankStatement>>();
        _mockBankAccountReadRepo = new Mock<IReadRepository<BankAccount>>();

        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateBankStatementHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateBankStatementHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetBankStatementHandler>>();

        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class =>
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] args) => new LocalizedString(name, name));

    private BankAccount CreateSampleBankAccount(Guid id, string accName = "Test Bank Acc", bool isActive = true) =>
        new BankAccount(accName, "BNKACC001", "Bank Corp", "USD", Guid.NewGuid(), null, isActive) { Id = id };

    private BankStatement CreateSampleBankStatement(Guid id, Guid bankAccountId, string refNum = "STMT001") =>
        new BankStatement(bankAccountId, DateTime.UtcNow.Date, 100m, 200m, refNum) { Id = id };

    // === CreateBankStatementHandler Tests ===
    [Fact]
    public async Task CreateBankStatementHandler_Should_CreateStatement_WhenValid()
    {
        var bankAccountId = Guid.NewGuid();
        var request = new CreateBankStatementRequest
        {
            BankAccountId = bankAccountId, StatementDate = DateTime.UtcNow.Date, OpeningBalance = 100, ClosingBalance = 250,
            Transactions = new List<CreateBankStatementTransactionRequestItem>
            {
                new CreateBankStatementTransactionRequestItem { TransactionDate = DateTime.UtcNow.Date, Description = "Credit", Amount = 150, Type = BankTransactionType.Credit },
            }
        };
        var bankAccount = CreateSampleBankAccount(bankAccountId);

        _mockBankAccountReadRepo.Setup(r => r.GetByIdAsync(bankAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(bankAccount);
        _mockStatementRepo.Setup(r => r.AddAsync(It.IsAny<BankStatement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BankStatement bs, CancellationToken ct) => { bs.Id = Guid.NewGuid(); return bs; });

        var handler = new CreateBankStatementHandler(_mockStatementRepo.Object, _mockBankAccountReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeEmpty();
        _mockStatementRepo.Verify(r => r.AddAsync(It.Is<BankStatement>(bs => bs.BankAccountId == bankAccountId && bs.Transactions.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBankStatementHandler_Should_ThrowNotFound_WhenBankAccountNotFound()
    {
        var request = new CreateBankStatementRequest { BankAccountId = Guid.NewGuid() };
        _mockBankAccountReadRepo.Setup(r => r.GetByIdAsync(request.BankAccountId, It.IsAny<CancellationToken>())).ReturnsAsync((BankAccount)null!);
        var handler = new CreateBankStatementHandler(_mockStatementRepo.Object, _mockBankAccountReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateBankStatementHandler_Should_ThrowValidation_WhenBalancesDoNotReconcile()
    {
        var bankAccountId = Guid.NewGuid();
        var request = new CreateBankStatementRequest // Balances and transactions mismatch
        {
            BankAccountId = bankAccountId, StatementDate = DateTime.UtcNow.Date, OpeningBalance = 100, ClosingBalance = 200, // Diff = 100
            Transactions = new List<CreateBankStatementTransactionRequestItem>
            { new CreateBankStatementTransactionRequestItem { TransactionDate = DateTime.UtcNow.Date, Description = "Credit", Amount = 50, Type = BankTransactionType.Credit } } // Sum = 50
        };
        var bankAccount = CreateSampleBankAccount(bankAccountId);
        _mockBankAccountReadRepo.Setup(r => r.GetByIdAsync(bankAccountId, It.IsAny<CancellationToken>())).ReturnsAsync(bankAccount);
        var handler = new CreateBankStatementHandler(_mockStatementRepo.Object, _mockBankAccountReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === GetBankStatementHandler Tests ===
    [Fact]
    public async Task GetBankStatementHandler_Should_ReturnDtoWithDetails_WhenFound()
    {
        var statementId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var request = new GetBankStatementRequest(statementId);

        var bankAccountEntity = CreateSampleBankAccount(bankAccountId, "My Checking");
        var statementEntity = CreateSampleBankStatement(statementId, bankAccountId, "STMT-GET-01");
        // Simulate Include(bs => bs.BankAccount)
        typeof(BankStatement).GetProperty("BankAccount")!.SetValue(statementEntity, bankAccountEntity);
        // Simulate Include(bs => bs.Transactions)
        var transaction = new BankStatementTransaction(statementId, DateTime.UtcNow, "Tx Desc", 50, BankTransactionType.Credit);
        typeof(BankStatement).GetMethod("AddTransaction")!.Invoke(statementEntity, new object[] { transaction });


        // The spec BankStatementByIdWithDetailsSpec is Specification<BankStatement, BankStatementDto>
        // So FirstOrDefaultAsync(spec) should return BankStatementDto directly if Mapster projection is setup.
        // If not, it returns BankStatement entity, and handler adapts.
        // Current GetBankStatementHandler code implies spec returns DTO.
        var statementDto = statementEntity.Adapt<BankStatementDto>(); // Simulate what spec would return
        statementDto.BankAccountName = $"{bankAccountEntity.BankName} - {bankAccountEntity.AccountNumber}"; // Manual mapping for test if not in Mapster config
        statementDto.Transactions.First().Type = transaction.Type.ToString();


        _mockStatementReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<BankStatementByIdWithDetailsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statementDto);

        var handler = new GetBankStatementHandler(_mockStatementReadRepo.Object, _mockGetLocalizer.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(statementId);
        result.BankAccountName.Should().Be(statementDto.BankAccountName);
        result.Transactions.Should().HaveCount(1);
        result.Transactions.First().Description.Should().Be("Tx Desc");
    }

    // === SearchBankStatementsHandler Tests ===
    [Fact]
    public async Task SearchBankStatementsHandler_Should_ReturnPaginatedDtosWithBankAccountName()
    {
        var request = new SearchBankStatementsRequest { PageNumber = 1, PageSize = 10 };
        var bankAccountId1 = Guid.NewGuid();
        var bankAccount1 = CreateSampleBankAccount(bankAccountId1, "Search Bank Acc");

        // Assuming spec returns DTOs and Mapster handles BankAccount.AccountName -> BankAccountName by convention
        var statementsDtoList = new List<BankStatementDto>
        {
            new BankStatement(bankAccountId1, DateTime.UtcNow, 100, 200, "S001").Adapt<BankStatementDto>(),
            new BankStatement(bankAccountId1, DateTime.UtcNow.AddDays(-1), 200, 300, "S002").Adapt<BankStatementDto>()
        };
        statementsDtoList.ForEach(dto => dto.BankAccountName = $"{bankAccount1.BankName} - {bankAccount1.AccountNumber}"); // Simulate enrichment

        _mockStatementReadRepo.Setup(r => r.ListAsync(It.IsAny<BankStatementsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statementsDtoList);
        _mockStatementReadRepo.Setup(r => r.CountAsync(It.IsAny<BankStatementsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statementsDtoList.Count);

        var handler = new SearchBankStatementsHandler(_mockStatementReadRepo.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(statementsDtoList.Count);
        result.Data.All(dto => dto.BankAccountName == $"{bankAccount1.BankName} - {bankAccount1.AccountNumber}").Should().BeTrue();
    }
}
