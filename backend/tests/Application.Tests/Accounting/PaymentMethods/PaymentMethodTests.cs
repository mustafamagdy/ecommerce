using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For PaymentMethod
using System;

namespace Application.Tests.Accounting.PaymentMethods;

public class PaymentMethodTests
{
    private PaymentMethod CreateTestPaymentMethod(
        string name = "Credit Card",
        string? description = "Payment by credit card",
        bool isActive = true)
    {
        return new PaymentMethod(name, description, isActive);
    }

    [Fact]
    public void Constructor_Should_InitializePaymentMethodCorrectly()
    {
        // Arrange
        var name = "Bank Transfer";
        var desc = "Direct bank transfer (ACH/SEPA)";
        var isActive = true;

        // Act
        var method = new PaymentMethod(name, desc, isActive);

        // Assert
        method.Id.Should().NotBe(Guid.Empty);
        method.Name.Should().Be(name);
        method.Description.Should().Be(desc);
        method.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void Update_Should_ModifyPropertiesCorrectly()
    {
        // Arrange
        var method = CreateTestPaymentMethod();
        var originalId = method.Id;

        var newName = "PayPal";
        var newDesc = "Payment via PayPal services";
        var newIsActive = false;

        // Act
        method.Update(newName, newDesc, newIsActive);

        // Assert
        method.Id.Should().Be(originalId); // ID should not change
        method.Name.Should().Be(newName);
        method.Description.Should().Be(newDesc);
        method.IsActive.Should().Be(newIsActive);
    }

    [Fact]
    public void Update_WithNullDescription_Should_SetDescriptionToNull()
    {
        // Arrange
        var method = CreateTestPaymentMethod(description: "Initial Description");

        // Act
        method.Update(name: null, description: null, isActive: null); // Only update description to null

        // Assert
        method.Description.Should().BeNull();
        // Other properties should remain unchanged from helper defaults
        method.Name.Should().Be("Credit Card");
        method.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_OnlySomeProperties_Should_LeaveOthersUnchanged()
    {
        // Arrange
        var originalName = "Check";
        var originalDesc = "Payment by physical check";
        var originalIsActive = true;
        var method = new PaymentMethod(originalName, originalDesc, originalIsActive);

        var newName = "Certified Check";
        var newIsActive = false;

        // Act
        method.Update(newName, null, newIsActive);

        // Assert
        method.Name.Should().Be(newName);
        method.Description.Should().Be(originalDesc); // Unchanged
        method.IsActive.Should().Be(newIsActive);
    }
}
