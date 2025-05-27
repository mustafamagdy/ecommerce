using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.JournalEntries;
using FSH.WebApi.Domain.Accounting; // For TransactionType enum
using System;
using System.Collections.Generic;
using Xunit;

namespace FSH.WebApi.Application.Tests.Accounting.JournalEntries;

public class CreateJournalEntryRequestValidatorTests
{
    private readonly CreateJournalEntryRequestValidator _validator;

    public CreateJournalEntryRequestValidatorTests()
    {
        _validator = new CreateJournalEntryRequestValidator();
    }

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Valid Journal Entry",
            ReferenceNumber = "REF001",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Debit.ToString(), Amount = 100, Description = "Debit Leg 1" },
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Credit.ToString(), Amount = 100, Description = "Credit Leg 1" }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_EntryDate_Is_Default()
    {
        var request = new CreateJournalEntryRequest { EntryDate = default, Description = "Test", Transactions = ValidTransactions() };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.EntryDate);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Null()
    {
        var request = new CreateJournalEntryRequest { EntryDate = DateTime.UtcNow, Description = null!, Transactions = ValidTransactions() };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Empty()
    {
        var request = new CreateJournalEntryRequest { EntryDate = DateTime.UtcNow, Description = string.Empty, Transactions = ValidTransactions() };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_MaxLength()
    {
        var request = new CreateJournalEntryRequest { EntryDate = DateTime.UtcNow, Description = new string('a', 1025), Transactions = ValidTransactions() };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }


    [Fact]
    public void Should_Have_Error_When_Transactions_List_Is_Empty()
    {
        var request = new CreateJournalEntryRequest { EntryDate = DateTime.UtcNow, Description = "Test", Transactions = new List<CreateTransactionRequestItem>() };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Transactions)
            .WithErrorMessage("At least one transaction is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Transaction_AccountId_Is_Empty()
    {
        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Test",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = Guid.Empty, TransactionType = TransactionType.Debit.ToString(), Amount = 100 }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Transactions[0].AccountId");
    }

    [Fact]
    public void Should_Have_Error_When_Transaction_TransactionType_Is_Invalid()
    {
        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Test",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = "Invalid", Amount = 100 }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Transactions[0].TransactionType")
            .WithErrorMessage("Invalid Transaction Type. Must be 'Debit' or 'Credit'.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Should_Have_Error_When_Transaction_Amount_Is_Zero_Or_Negative(decimal amount)
    {
        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Test",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Debit.ToString(), Amount = amount }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Transactions[0].Amount")
            .WithErrorMessage("Amount must be positive.");
    }

    [Fact]
    public void Should_Have_Error_When_Debits_Do_Not_Equal_Credits()
    {
        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Unbalanced Entry",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Debit.ToString(), Amount = 100 },
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Credit.ToString(), Amount = 90 } // Unbalanced
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Transactions)
            .WithErrorMessage("Debits must equal Credits.");
    }

    [Fact]
    public void Should_Pass_When_Debits_Equal_Credits_With_Multiple_Transactions()
    {
        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Balanced Entry",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Debit.ToString(), Amount = 100 },
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Debit.ToString(), Amount = 50 },
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Credit.ToString(), Amount = 75 },
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Credit.ToString(), Amount = 75 }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Transactions); // Specifically for the balance rule
        result.ShouldNotHaveAnyValidationErrors(); // Overall
    }

    private List<CreateTransactionRequestItem> ValidTransactions() => new List<CreateTransactionRequestItem>
    {
        new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Debit.ToString(), Amount = 100 },
        new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Credit.ToString(), Amount = 100 }
    };
}
