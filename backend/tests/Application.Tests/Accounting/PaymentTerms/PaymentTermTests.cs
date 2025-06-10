using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For PaymentTerm
using System;

namespace Application.Tests.Accounting.PaymentTerms;

public class PaymentTermTests
{
    private PaymentTerm CreateTestPaymentTerm(
        string name = "Net 30",
        int daysUntilDue = 30,
        string? description = "Payment due in 30 days",
        bool isActive = true)
    {
        return new PaymentTerm(name, daysUntilDue, description, isActive);
    }

    [Fact]
    public void Constructor_Should_InitializePaymentTermCorrectly()
    {
        // Arrange
        var name = "Net 15";
        var days = 15;
        var desc = "Due in 15 days";
        var isActive = false;

        // Act
        var term = new PaymentTerm(name, days, desc, isActive);

        // Assert
        term.Id.Should().NotBe(Guid.Empty);
        term.Name.Should().Be(name);
        term.DaysUntilDue.Should().Be(days);
        term.Description.Should().Be(desc);
        term.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void Update_Should_ModifyPropertiesCorrectly()
    {
        // Arrange
        var term = CreateTestPaymentTerm();
        var originalId = term.Id;

        var newName = "Net 60";
        var newDays = 60;
        var newDesc = "Payment due in 60 days";
        var newIsActive = false;

        // Act
        term.Update(newName, newDays, newDesc, newIsActive);

        // Assert
        term.Id.Should().Be(originalId); // ID should not change
        term.Name.Should().Be(newName);
        term.DaysUntilDue.Should().Be(newDays);
        term.Description.Should().Be(newDesc);
        term.IsActive.Should().Be(newIsActive);
    }

    [Fact]
    public void Update_WithNullDescription_Should_SetDescriptionToNull()
    {
        // Arrange
        var term = CreateTestPaymentTerm(description: "Initial Description");

        // Act
        term.Update(name: null, daysUntilDue: null, description: null, isActive: null); // Only update description to null

        // Assert
        term.Description.Should().BeNull();
        // Other properties should remain unchanged from helper defaults
        term.Name.Should().Be("Net 30");
        term.DaysUntilDue.Should().Be(30);
        term.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_OnlySomeProperties_Should_LeaveOthersUnchanged()
    {
        // Arrange
        var originalName = "Net 30";
        var originalDays = 30;
        var originalDesc = "Original Description";
        var originalIsActive = true;
        var term = new PaymentTerm(originalName, originalDays, originalDesc, originalIsActive);

        var newName = "Net 30 Updated";
        var newIsActive = false;

        // Act
        term.Update(newName, null, null, newIsActive);

        // Assert
        term.Name.Should().Be(newName);
        term.DaysUntilDue.Should().Be(originalDays);         // Unchanged
        term.Description.Should().Be(originalDesc);       // Unchanged
        term.IsActive.Should().Be(newIsActive);
    }

    [Fact]
    public void Constructor_DaysUntilDueIsZero_Should_BeValid()
    {
        // Arrange & Act
        var term = new PaymentTerm("Due on Receipt", 0, "Payment is due immediately", true);

        // Assert
        term.DaysUntilDue.Should().Be(0);
    }

    // Assuming PaymentTerm domain entity might have validation for DaysUntilDue > 0 in constructor or update
    // Based on current PaymentTerm.cs, it does not throw for negative, but CreateAssetCategoryRequestValidator has Range(1,100) for similar fields.
    // If domain rules were stricter (e.g. DaysUntilDue must be >= 0), we'd test for ArgumentOutOfRangeException.
    // The current PaymentTerm entity does not have such validation, only AssetCategory's DefaultUsefulLifeYears had it.
}
