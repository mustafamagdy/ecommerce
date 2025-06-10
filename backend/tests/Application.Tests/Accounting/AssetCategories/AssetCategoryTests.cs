using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For AssetCategory, DepreciationMethod
using System;

namespace Application.Tests.Accounting.AssetCategories;

public class AssetCategoryTests
{
    private AssetCategory CreateTestAssetCategory(
        string name = "Office Equipment",
        string? description = "Standard office equipment category",
        Guid? defaultDepreciationMethodId = null,
        int? defaultUsefulLifeYears = 5,
        bool isActive = true)
    {
        return new AssetCategory(
            name,
            description,
            defaultDepreciationMethodId ?? Guid.NewGuid(), // Ensure a Guid if not provided, or handle null
            defaultUsefulLifeYears,
            isActive);
    }

    [Fact]
    public void Constructor_Should_InitializeAssetCategoryCorrectly_WithAllValues()
    {
        // Arrange
        var name = "Vehicles";
        var description = "Company vehicles";
        var depMethodId = Guid.NewGuid();
        var usefulLife = 7;
        var isActive = true;

        // Act
        var category = new AssetCategory(name, description, depMethodId, usefulLife, isActive);

        // Assert
        category.Id.Should().NotBe(Guid.Empty);
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
        category.DefaultDepreciationMethodId.Should().Be(depMethodId);
        category.DefaultUsefulLifeYears.Should().Be(usefulLife);
        category.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void Constructor_Should_InitializeAssetCategoryCorrectly_WithOptionalValuesNull()
    {
        // Arrange
        var name = "Furniture";
        var isActive = false;

        // Act
        var category = new AssetCategory(name, null, null, null, isActive);

        // Assert
        category.Name.Should().Be(name);
        category.Description.Should().BeNull();
        category.DefaultDepreciationMethodId.Should().BeNull();
        category.DefaultUsefulLifeYears.Should().BeNull();
        category.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void Constructor_WithNegativeDefaultUsefulLife_Should_ThrowArgumentOutOfRangeException()
    {
        // Arrange
        Action act = () => new AssetCategory("Test", null, null, -1, true);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("defaultUsefulLifeYears");
    }

    [Fact]
    public void Constructor_WithZeroDefaultUsefulLife_Should_ThrowArgumentOutOfRangeException()
    {
        // Arrange
        Action act = () => new AssetCategory("Test", null, null, 0, true);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("defaultUsefulLifeYears");
    }


    [Fact]
    public void Update_Should_ModifyAllEditableProperties()
    {
        // Arrange
        var category = CreateTestAssetCategory();
        var originalId = category.Id;

        var newName = "Machinery";
        var newDescription = "Heavy industrial machinery";
        var newDepMethodId = Guid.NewGuid();
        var newUsefulLife = 10;
        var newIsActive = false;

        // Act
        category.Update(newName, newDescription, newDepMethodId, newUsefulLife, newIsActive);

        // Assert
        category.Id.Should().Be(originalId);
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
        category.DefaultDepreciationMethodId.Should().Be(newDepMethodId);
        category.DefaultUsefulLifeYears.Should().Be(newUsefulLife);
        category.IsActive.Should().Be(newIsActive);
    }

    [Fact]
    public void Update_WithNullOptionalValues_Should_SetPropertiesToNull()
    {
        // Arrange
        var category = CreateTestAssetCategory(
            defaultDepreciationMethodId: Guid.NewGuid(),
            defaultUsefulLifeYears: 5,
            description: "Initial Description");

        // Act
        category.Update(
            name: null, // Name will not change due to domain logic: if (name is not null ...)
            description: null,
            defaultDepreciationMethodId: null,
            defaultUsefulLifeYears: null,
            isActive: null // IsActive will not change
            );

        // Assert
        category.Name.Should().Be("Office Equipment"); // Default from helper, unchanged by null
        category.Description.Should().BeNull();
        category.DefaultDepreciationMethodId.Should().BeNull();
        category.DefaultUsefulLifeYears.Should().BeNull();
        category.IsActive.Should().BeTrue(); // Default from helper, unchanged by null
    }

    [Fact]
    public void Update_OnlySomeProperties_Should_LeaveOthersUnchanged()
    {
        // Arrange
        var originalName = "Computers";
        var originalDesc = "Desktop and laptop computers";
        var originalDepMethodId = Guid.NewGuid();
        var originalLife = 3;
        var originalIsActive = true;
        var category = new AssetCategory(originalName, originalDesc, originalDepMethodId, originalLife, originalIsActive);

        var newName = "Updated Computers";
        var newIsActive = false;

        // Act
        category.Update(newName, null, null, null, newIsActive);

        // Assert
        category.Name.Should().Be(newName);
        category.Description.Should().Be(originalDesc); // Unchanged
        category.DefaultDepreciationMethodId.Should().Be(originalDepMethodId); // Unchanged
        category.DefaultUsefulLifeYears.Should().Be(originalLife); // Unchanged
        category.IsActive.Should().Be(newIsActive);
    }

    [Fact]
    public void Update_WithNegativeDefaultUsefulLife_Should_ThrowArgumentOutOfRangeException()
    {
        // Arrange
        var category = CreateTestAssetCategory();
        Action act = () => category.Update(null, null, null, -2, null);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("defaultUsefulLifeYears");
    }

    [Fact]
    public void Update_WithZeroDefaultUsefulLife_Should_ThrowArgumentOutOfRangeException()
    {
        // Arrange
        var category = CreateTestAssetCategory();
        Action act = () => category.Update(null, null, null, 0, null);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("defaultUsefulLifeYears");
    }
}
