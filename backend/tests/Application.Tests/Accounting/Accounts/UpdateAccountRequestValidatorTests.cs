using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.Accounts;
using FSH.WebApi.Domain.Accounting; // For AccountType enum
using System;
using Xunit;

namespace FSH.WebApi.Application.Tests.Accounting.Accounts;

public class UpdateAccountRequestValidatorTests
{
    private readonly UpdateAccountRequestValidator _validator;

    public UpdateAccountRequestValidatorTests()
    {
        _validator = new UpdateAccountRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var request = new UpdateAccountRequest { Id = Guid.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Id_Is_Valid()
    {
        var request = new UpdateAccountRequest { Id = Guid.NewGuid() }; // Only Id is required for this specific check
        var result = _validator.TestValidate(request);
        // We expect errors for other fields if they are null and mandatory, but not for Id itself.
        // The validator only checks Id is not empty. Other rules are conditional.
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Have_Error_When_AccountName_Exceeds_MaxLength()
    {
        // Assuming MaxLength is 256 as per validator
        var request = new UpdateAccountRequest { Id = Guid.NewGuid(), AccountName = new string('a', 257) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_AccountName_Is_Null()
    {
        // AccountName is optional for update
        var request = new UpdateAccountRequest { Id = Guid.NewGuid(), AccountName = null };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.AccountName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_AccountName_Is_Valid()
    {
        var request = new UpdateAccountRequest { Id = Guid.NewGuid(), AccountName = "Valid Name" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.AccountName);
    }


    [Fact]
    public void Should_Have_Error_When_AccountNumber_Exceeds_MaxLength()
    {
        // Assuming MaxLength is 50 as per validator
        var request = new UpdateAccountRequest { Id = Guid.NewGuid(), AccountNumber = new string('1', 51) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountNumber);
    }

    [Fact]
    public void Should_Not_Have_Error_When_AccountNumber_Is_Null()
    {
        // AccountNumber is optional for update
        var request = new UpdateAccountRequest { Id = Guid.NewGuid(), AccountNumber = null };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.AccountNumber);
    }

    [Fact]
    public void Should_Not_Have_Error_When_AccountNumber_Is_Valid()
    {
        var request = new UpdateAccountRequest { Id = Guid.NewGuid(), AccountNumber = "12345" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.AccountNumber);
    }


    [Fact]
    public void Should_Have_Error_When_AccountType_Is_Invalid_Enum()
    {
        var request = new UpdateAccountRequest { Id = Guid.NewGuid(), AccountType = "InvalidType" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountType)
            .WithErrorMessage("Invalid Account Type.");
    }

    [Theory]
    [InlineData("Asset")]
    [InlineData("Liability")]
    [InlineData("Equity")]
    [InlineData("Revenue")]
    [InlineData("Expense")]
    [InlineData("asset")] // Case-insensitivity test
    public void Should_Not_Have_Error_When_AccountType_Is_Valid_Enum(string validAccountType)
    {
        var request = new UpdateAccountRequest { Id = Guid.NewGuid(), AccountType = validAccountType };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.AccountType);
    }

    [Fact]
    public void Should_Not_Have_Error_When_AccountType_Is_Null()
    {
        // AccountType is optional for update
        var request = new UpdateAccountRequest { Id = Guid.NewGuid(), AccountType = null };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.AccountType);
    }

    [Fact]
    public void Should_Pass_When_Request_Is_Valid_With_All_Fields()
    {
        var request = new UpdateAccountRequest
        {
            Id = Guid.NewGuid(),
            AccountName = "Valid Account Updated",
            AccountNumber = "ACC123Updated",
            AccountType = AccountType.Liability.ToString(),
            Description = "Updated description",
            IsActive = false
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_Request_Is_Valid_With_Only_Id_And_Optional_Fields_Null()
    {
        // Only Id is strictly required, other fields are optional for update.
        var request = new UpdateAccountRequest
        {
            Id = Guid.NewGuid(),
            AccountName = null,
            AccountNumber = null,
            AccountType = null,
            Description = null,
            IsActive = null
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
