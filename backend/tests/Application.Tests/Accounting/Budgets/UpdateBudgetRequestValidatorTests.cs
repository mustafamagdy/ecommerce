using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.Budgets;
using System;
using Xunit;

namespace FSH.WebApi.Application.Tests.Accounting.Budgets;

public class UpdateBudgetRequestValidatorTests
{
    private readonly UpdateBudgetRequestValidator _validator;

    public UpdateBudgetRequestValidatorTests()
    {
        _validator = new UpdateBudgetRequestValidator();
    }

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        var request = new UpdateBudgetRequest
        {
            Id = Guid.NewGuid(),
            BudgetName = "Updated Annual Marketing Budget",
            AccountId = Guid.NewGuid(),
            PeriodStartDate = new DateTime(2023, 1, 1),
            PeriodEndDate = new DateTime(2023, 12, 31),
            Amount = 55000,
            Description = "Updated marketing budget for FY2023"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var request = new UpdateBudgetRequest { Id = Guid.Empty, BudgetName = "Test", AccountId = Guid.NewGuid(), PeriodStartDate = DateTime.UtcNow, PeriodEndDate = DateTime.UtcNow.AddDays(1), Amount = 100 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Have_Error_When_BudgetName_Is_Null()
    {
        var request = new UpdateBudgetRequest { Id = Guid.NewGuid(), BudgetName = null!, AccountId = Guid.NewGuid(), PeriodStartDate = DateTime.UtcNow, PeriodEndDate = DateTime.UtcNow.AddDays(1), Amount = 100 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BudgetName);
    }

    [Fact]
    public void Should_Have_Error_When_BudgetName_Is_Empty()
    {
        var request = new UpdateBudgetRequest { Id = Guid.NewGuid(), BudgetName = string.Empty, AccountId = Guid.NewGuid(), PeriodStartDate = DateTime.UtcNow, PeriodEndDate = DateTime.UtcNow.AddDays(1), Amount = 100 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BudgetName);
    }

    [Fact]
    public void Should_Have_Error_When_BudgetName_Exceeds_MaxLength()
    {
        var request = new UpdateBudgetRequest { Id = Guid.NewGuid(), BudgetName = new string('a', 257), AccountId = Guid.NewGuid(), PeriodStartDate = DateTime.UtcNow, PeriodEndDate = DateTime.UtcNow.AddDays(1), Amount = 100 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BudgetName);
    }

    [Fact]
    public void Should_Have_Error_When_AccountId_Is_Empty()
    {
        var request = new UpdateBudgetRequest { Id = Guid.NewGuid(), BudgetName = "Test Budget", AccountId = Guid.Empty, PeriodStartDate = DateTime.UtcNow, PeriodEndDate = DateTime.UtcNow.AddDays(1), Amount = 100 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountId);
    }

    [Fact]
    public void Should_Have_Error_When_PeriodStartDate_Is_Default()
    {
        var request = new UpdateBudgetRequest { Id = Guid.NewGuid(), BudgetName = "Test Budget", AccountId = Guid.NewGuid(), PeriodStartDate = default, PeriodEndDate = DateTime.UtcNow.AddDays(1), Amount = 100 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PeriodStartDate);
    }

    [Fact]
    public void Should_Have_Error_When_PeriodEndDate_Is_Default()
    {
        var request = new UpdateBudgetRequest { Id = Guid.NewGuid(), BudgetName = "Test Budget", AccountId = Guid.NewGuid(), PeriodStartDate = DateTime.UtcNow, PeriodEndDate = default, Amount = 100 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PeriodEndDate);
    }

    [Fact]
    public void Should_Have_Error_When_PeriodEndDate_Is_Before_PeriodStartDate()
    {
        var request = new UpdateBudgetRequest
        {
            Id = Guid.NewGuid(),
            BudgetName = "Test Budget",
            AccountId = Guid.NewGuid(),
            PeriodStartDate = new DateTime(2023, 12, 31),
            PeriodEndDate = new DateTime(2023, 1, 1),
            Amount = 100
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PeriodEndDate)
            .WithErrorMessage("Period End Date must be after Period Start Date.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Should_Have_Error_When_Amount_Is_Zero_Or_Negative(decimal amount)
    {
        var request = new UpdateBudgetRequest
        {
            Id = Guid.NewGuid(),
            BudgetName = "Test Budget",
            AccountId = Guid.NewGuid(),
            PeriodStartDate = DateTime.UtcNow,
            PeriodEndDate = DateTime.UtcNow.AddDays(1),
            Amount = amount
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be positive.");
    }
}
