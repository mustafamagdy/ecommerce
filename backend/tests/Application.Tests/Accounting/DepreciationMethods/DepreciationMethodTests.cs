using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For DepreciationMethod
using System;

namespace Application.Tests.Accounting.DepreciationMethods;

public class DepreciationMethodTests
{
    private DepreciationMethod CreateTestDepreciationMethod(
        string name = "Straight-Line",
        string? description = "Allocates the cost of an asset evenly over its useful life.")
    {
        return new DepreciationMethod(name, description);
    }

    [Fact]
    public void Constructor_Should_InitializeDepreciationMethodCorrectly()
    {
        // Arrange
        var name = "Double Declining Balance";
        var description = "An accelerated depreciation method.";

        // Act
        var method = new DepreciationMethod(name, description);

        // Assert
        method.Id.Should().NotBe(Guid.Empty);
        method.Name.Should().Be(name);
        method.Description.Should().Be(description);
        // AuditableEntity properties (CreatedOn etc.) are handled by the base class/EF.
    }

    [Fact]
    public void Constructor_WithNullDescription_Should_InitializeDescriptionAsNull()
    {
        // Arrange
        var name = "Sum-of-the-Years' Digits";

        // Act
        var method = new DepreciationMethod(name, null);

        // Assert
        method.Name.Should().Be(name);
        method.Description.Should().BeNull();
    }

    [Fact]
    public void Update_Should_ModifyNameAndDescription()
    {
        // Arrange
        var method = CreateTestDepreciationMethod();
        var originalId = method.Id;

        var newName = "Units of Production";
        var newDescription = "Depreciation based on actual usage.";

        // Act
        method.Update(newName, newDescription);

        // Assert
        method.Id.Should().Be(originalId); // ID should not change
        method.Name.Should().Be(newName);
        method.Description.Should().Be(newDescription);
    }

    [Fact]
    public void Update_WithNullName_Should_NotChangeName_AsNameIsLikelyRequired()
    {
        // Arrange
        var originalName = "Straight-Line Original";
        var method = CreateTestDepreciationMethod(name: originalName);
        var newDescription = "Only description updated.";

        // Act
        // The Update method on DepreciationMethod is: Update(string? name, string? description)
        // if (name is not null && Name?.Equals(name) is not true) Name = name;
        // So passing null for name means it won't update the name.
        method.Update(null, newDescription);

        // Assert
        method.Name.Should().Be(originalName);
        method.Description.Should().Be(newDescription);
    }

    [Fact]
    public void Update_WithNullDescription_Should_SetDescriptionToNull()
    {
        // Arrange
        var method = CreateTestDepreciationMethod(description: "Initial Description");

        // Act
        method.Update(null, null); // Only update description to null (name also null, so name unchanged)

        // Assert
        method.Description.Should().BeNull();
        method.Name.Should().Be("Straight-Line"); // Default from helper
    }

    [Fact]
    public void Update_OnlyName_Should_LeaveDescriptionUnchanged()
    {
        // Arrange
        var originalDescription = "Original Description for SL";
        var method = CreateTestDepreciationMethod(description: originalDescription);
        var newName = "Straight-Line Modified";

        // Act
        method.Update(newName, null); // Description is null, so it won't update description.

        // Assert
        method.Name.Should().Be(newName);
        method.Description.Should().Be(originalDescription);
    }
}
