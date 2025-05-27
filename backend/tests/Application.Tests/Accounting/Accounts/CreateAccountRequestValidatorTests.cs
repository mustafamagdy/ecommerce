using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.Accounts;
using FSH.WebApi.Domain.Accounting; // For AccountType enum
using Xunit;

namespace FSH.WebApi.Application.Tests.Accounting.Accounts;

public class CreateAccountRequestValidatorTests
{
    private readonly CreateAccountRequestValidator _validator;

    public CreateAccountRequestValidatorTests()
    {
        _validator = new CreateAccountRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_AccountName_Is_Null()
    {
        var request = new CreateAccountRequest { AccountName = null!, AccountNumber = "123", AccountType = AccountType.Asset.ToString() };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountName);
    }

    [Fact]
    public void Should_Have_Error_When_AccountName_Is_Empty()
    {
        var request = new CreateAccountRequest { AccountName = string.Empty, AccountNumber = "123", AccountType = AccountType.Asset.ToString() };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountName);
    }

    [Fact]
    public void Should_Have_Error_When_AccountName_Exceeds_MaxLength()
    {
        // Assuming MaxLength is 256 as per validator
        var request = new CreateAccountRequest { AccountName = new string('a', 257), AccountNumber = "123", AccountType = AccountType.Asset.ToString() };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_AccountName_Is_Valid()
    {
        var request = new CreateAccountRequest { AccountName = "Valid Name", AccountNumber = "123", AccountType = AccountType.Asset.ToString() };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.AccountName);
    }

    [Fact]
    public void Should_Have_Error_When_AccountNumber_Is_Null()
    {
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = null!, AccountType = AccountType.Asset.ToString() };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountNumber);
    }

    [Fact]
    public void Should_Have_Error_When_AccountNumber_Is_Empty()
    {
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = string.Empty, AccountType = AccountType.Asset.ToString() };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountNumber);
    }

    [Fact]
    public void Should_Have_Error_When_AccountNumber_Exceeds_MaxLength()
    {
        // Assuming MaxLength is 50 as per validator
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = new string('1', 51), AccountType = AccountType.Asset.ToString() };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountNumber);
    }

    [Fact]
    public void Should_Not_Have_Error_When_AccountNumber_Is_Valid()
    {
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = "12345", AccountType = AccountType.Asset.ToString() };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.AccountNumber);
    }

    [Fact]
    public void Should_Have_Error_When_AccountType_Is_Null()
    {
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = "123", AccountType = null! };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountType);
    }

    [Fact]
    public void Should_Have_Error_When_AccountType_Is_Empty()
    {
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = "123", AccountType = string.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccountType);
    }

    [Fact]
    public void Should_Have_Error_When_AccountType_Is_Invalid_Enum()
    {
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = "123", AccountType = "InvalidType" };
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
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = "123", AccountType = validAccountType };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.AccountType);
    }

    [Fact]
    public void Should_Have_Error_When_InitialBalance_Is_Negative()
    {
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = "123", AccountType = AccountType.Asset.ToString(), InitialBalance = -100 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InitialBalance);
    }

    [Fact]
    public void Should_Not_Have_Error_When_InitialBalance_Is_Zero()
    {
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = "123", AccountType = AccountType.Asset.ToString(), InitialBalance = 0 };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.InitialBalance);
    }

    [Fact]
    public void Should_Not_Have_Error_When_InitialBalance_Is_Positive()
    {
        var request = new CreateAccountRequest { AccountName = "Test", AccountNumber = "123", AccountType = AccountType.Asset.ToString(), InitialBalance = 100 };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.InitialBalance);
    }

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        var request = new CreateAccountRequest
        {
            AccountName = "Valid Account",
            AccountNumber = "ACC123",
            AccountType = AccountType.Asset.ToString(),
            InitialBalance = 1000,
            Description = "Valid description"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
