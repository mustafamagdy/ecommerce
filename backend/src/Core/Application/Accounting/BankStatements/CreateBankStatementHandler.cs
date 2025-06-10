using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq; // Required for .Sum()
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.BankStatements;

public class CreateBankStatementHandler : IRequestHandler<CreateBankStatementRequest, Guid>
{
    private readonly IRepository<BankStatement> _statementRepository;
    private readonly IReadRepository<BankAccount> _bankAccountRepository;
    private readonly IStringLocalizer<CreateBankStatementHandler> _localizer;
    private readonly ILogger<CreateBankStatementHandler> _logger;

    public CreateBankStatementHandler(
        IRepository<BankStatement> statementRepository,
        IReadRepository<BankAccount> bankAccountRepository,
        IStringLocalizer<CreateBankStatementHandler> localizer,
        ILogger<CreateBankStatementHandler> logger)
    {
        _statementRepository = statementRepository;
        _bankAccountRepository = bankAccountRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateBankStatementRequest request, CancellationToken cancellationToken)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAsync(request.BankAccountId, cancellationToken);
        if (bankAccount == null)
        {
            throw new NotFoundException(_localizer["Bank Account with ID {0} not found.", request.BankAccountId]);
        }
        if (!bankAccount.IsActive)
        {
            // throw new ValidationException(_localizer["Bank Account '{0}' is inactive.", bankAccount.AccountName]);
        }

        // Basic validation for balance reconciliation (also in validator, good to have server-side too)
        decimal creditSum = request.Transactions.Where(t => t.Type == BankTransactionType.Credit).Sum(t => t.Amount);
        decimal debitSum = request.Transactions.Where(t => t.Type == BankTransactionType.Debit).Sum(t => t.Amount);
        if (Math.Abs((request.ClosingBalance - request.OpeningBalance) - (creditSum - debitSum)) > 0.001m) // Using a small tolerance
        {
            throw new ValidationException(_localizer["Statement balances and transaction totals do not reconcile."]);
        }

        var bankStatement = new BankStatement(
            bankAccountId: request.BankAccountId,
            statementDate: request.StatementDate,
            openingBalance: request.OpeningBalance,
            closingBalance: request.ClosingBalance,
            referenceNumber: request.ReferenceNumber
        );
        // ImportDate is set in constructor

        foreach (var itemRequest in request.Transactions)
        {
            var transaction = new BankStatementTransaction(
                bankStatementId: bankStatement.Id, // Will be set by EF if relationship is auto-managed
                transactionDate: itemRequest.TransactionDate,
                description: itemRequest.Description,
                amount: itemRequest.Amount,
                type: itemRequest.Type,
                reference: itemRequest.Reference,
                bankProvidedId: itemRequest.BankProvidedId
            );
            bankStatement.AddTransaction(transaction);
        }

        await _statementRepository.AddAsync(bankStatement, cancellationToken);

        _logger.LogInformation(_localizer["Bank Statement for Account '{0}' dated {1} created."], bankAccount.AccountName, bankStatement.StatementDate.ToShortDateString());
        return bankStatement.Id;
    }
}
