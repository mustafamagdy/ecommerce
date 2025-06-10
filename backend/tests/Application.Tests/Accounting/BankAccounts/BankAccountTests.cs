using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For BankAccount
using System;

namespace Application.Tests.Accounting.BankAccounts;

public class BankAccountTests
{
    private BankAccount CreateTestBankAccount(
        string accountName = "Main Checking Account",
        string accountNumber = "1234567890",
        string bankName = "Global Bank Corp",
        string currency = "USD",
        Guid? glAccountId = null,
        string? branchName = "Downtown Branch",
        bool isActive = true)
    {
        return new BankAccount(
            accountName,
            accountNumber,
            bankName,
            currency,
            glAccountId ?? Guid.NewGuid(),
            branchName,
            isActive);
    }

    [Fact]
    public void Constructor_Should_InitializeBankAccountCorrectly_WithAllValues()
    {
        // Arrange
        var name = "Savings Account";
        var number = "00987654321";
        var bank = "Community Bank";
        var curr = "EUR";
        var glId = Guid.NewGuid();
        var branch = "Westside Branch";
        var active = false;

        // Act
        var account = new BankAccount(name, number, bank, curr, glId, branch, active);

        // Assert
        account.Id.Should().NotBe(Guid.Empty);
        account.AccountName.Should().Be(name);
        account.AccountNumber.Should().Be(number);
        account.BankName.Should().Be(bank);
        account.Currency.Should().Be(curr);
        account.GLAccountId.Should().Be(glId);
        account.BranchName.Should().Be(branch);
        account.IsActive.Should().Be(active);
    }

    [Fact]
    public void Constructor_WithNullOptionalValues_Should_InitializeCorrectly()
    {
        // Arrange
        var name = "Basic Account";
        var number = "BASIC001";
        var bank = "Simple Bank";
        var curr = "USD";
        var glId = Guid.NewGuid();

        // Act
        var account = new BankAccount(name, number, bank, curr, glId, null, true); // branchName is null

        // Assert
        account.AccountName.Should().Be(name);
        account.AccountNumber.Should().Be(number);
        account.BankName.Should().Be(bank);
        account.Currency.Should().Be(curr);
        account.GLAccountId.Should().Be(glId);
        account.BranchName.Should().BeNull();
        account.IsActive.Should().BeTrue();
    }


    [Fact]
    public void Update_Should_ModifyAllEditableProperties()
    {
        // Arrange
        var account = CreateTestBankAccount();
        var originalId = account.Id;

        var newName = "Primary Business Account";
        var newNumber = "9876500000";
        var newBank = "Enterprise Bank Ltd.";
        var newCurr = "GBP";
        var newGlId = Guid.NewGuid();
        var newBranch = "HQ Branch";
        var newIsActive = false;

        // Act
        account.Update(newName, newNumber, newBank, newCurr, newGlId, newBranch, newIsActive);

        // Assert
        account.Id.Should().Be(originalId);
        account.AccountName.Should().Be(newName);
        account.AccountNumber.Should().Be(newNumber);
        account.BankName.Should().Be(newBank);
        account.Currency.Should().Be(newCurr);
        account.GLAccountId.Should().Be(newGlId);
        account.BranchName.Should().Be(newBranch);
        account.IsActive.Should().Be(newIsActive);
    }

    [Fact]
    public void Update_WithNullValuesForOptionalFields_Should_SetThemToNullOrNotChange()
    {
        // Arrange
        var account = CreateTestBankAccount(branchName: "Initial Branch"); // Start with a non-null branch

        // Act
        account.Update(
            accountName: null, // Name will not change
            accountNumber: null, // Number will not change
            bankName: null,      // Bank name will not change
            currency: null,      // Currency will not change
            glAccountId: null,   // GL ID will not change
            branchName: "",      // Set branch to empty string (which Update method should treat as null or empty based on its logic)
                                 // The domain logic: else if (string.IsNullOrEmpty(branchName) && BranchName is not null) BranchName = null;
            isActive: null       // Active status will not change
        );

        // Assert
        account.AccountName.Should().Be("Main Checking Account"); // Default from helper
        account.BranchName.Should().BeNull(); // Set to null because empty string was passed
    }

    [Fact]
    public void Update_OnlySomeProperties_Should_LeaveOthersUnchanged()
    {
        // Arrange
        var originalName = "Original Account Name";
        var originalNumber = "ORIGINAL123";
        var originalBank = "Original Bank";
        var originalCurrency = "AUD";
        var originalGlId = Guid.NewGuid();
        var originalBranch = "Original Branch";
        var originalIsActive = true;

        var account = new BankAccount(originalName, originalNumber, originalBank, originalCurrency, originalGlId, originalBranch, originalIsActive);

        var newName = "New Account Name Only";
        var newIsActive = false;

        // Act
        account.Update(newName, null, null, null, null, null, newIsActive);

        // Assert
        account.AccountName.Should().Be(newName);
        account.AccountNumber.Should().Be(originalNumber);
        account.BankName.Should().Be(originalBank);
        account.Currency.Should().Be(originalCurrency);
        account.GLAccountId.Should().Be(originalGlId);
        account.BranchName.Should().Be(originalBranch);
        account.IsActive.Should().Be(newIsActive);
    }
}
