using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For AssetDepreciationEntry
using System;

namespace Application.Tests.Accounting.FixedAssets;

public class AssetDepreciationEntryTests
{
    [Fact]
    public void Constructor_Should_InitializeAssetDepreciationEntryCorrectly()
    {
        // Arrange
        var fixedAssetId = Guid.NewGuid();
        var depreciationDate = DateTime.UtcNow.Date;
        var amount = 100.50m;
        var journalEntryId = Guid.NewGuid();

        // Act
        var entry = new AssetDepreciationEntry(fixedAssetId, depreciationDate, amount, journalEntryId);

        // Assert
        entry.Id.Should().NotBe(Guid.Empty);
        entry.FixedAssetId.Should().Be(fixedAssetId);
        entry.DepreciationDate.Should().Be(depreciationDate);
        entry.Amount.Should().Be(amount);
        entry.JournalEntryId.Should().Be(journalEntryId);
        // AuditableEntity properties are handled by base class/EF.
    }

    [Fact]
    public void Constructor_WithNullJournalEntryId_Should_InitializeCorrectly()
    {
        // Arrange
        var fixedAssetId = Guid.NewGuid();
        var depreciationDate = DateTime.UtcNow.Date;
        var amount = 50m;

        // Act
        var entry = new AssetDepreciationEntry(fixedAssetId, depreciationDate, amount, null);

        // Assert
        entry.FixedAssetId.Should().Be(fixedAssetId);
        entry.DepreciationDate.Should().Be(depreciationDate);
        entry.Amount.Should().Be(amount);
        entry.JournalEntryId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNegativeAmount_Should_ThrowArgumentOutOfRangeException()
    {
        // Arrange
        Action act = () => new AssetDepreciationEntry(Guid.NewGuid(), DateTime.UtcNow, -100m, null);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("amount");
    }

    [Fact]
    public void Constructor_WithZeroAmount_Should_BeAllowed()
    {
        // Arrange & Act
        var entry = new AssetDepreciationEntry(Guid.NewGuid(), DateTime.UtcNow, 0m, null);

        // Assert
        entry.Amount.Should().Be(0m);
    }


    [Fact]
    public void Update_Should_ModifyProperties_WhenAllowedByBusinessRules()
    {
        // Arrange
        var entry = new AssetDepreciationEntry(Guid.NewGuid(), DateTime.UtcNow.Date, 100m, null);
        var newDate = DateTime.UtcNow.Date.AddMonths(-1);
        var newAmount = 120m;
        var newJournalId = Guid.NewGuid();

        // Act
        entry.Update(newDate, newAmount, newJournalId);

        // Assert
        entry.DepreciationDate.Should().Be(newDate);
        entry.Amount.Should().Be(newAmount);
        entry.JournalEntryId.Should().Be(newJournalId);
    }

    [Fact]
    public void Update_WithNegativeAmount_Should_ThrowArgumentOutOfRangeException()
    {
        // Arrange
        var entry = new AssetDepreciationEntry(Guid.NewGuid(), DateTime.UtcNow.Date, 100m, null);

        // Act
        Action act = () => entry.Update(null, -50m, null);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("amount");
    }

    [Fact]
    public void Update_WithNullValues_Should_NotChangeExistingValues()
    {
        // Arrange
        var originalDate = DateTime.UtcNow.Date;
        var originalAmount = 100m;
        var originalJournalId = Guid.NewGuid();
        var entry = new AssetDepreciationEntry(Guid.NewGuid(), originalDate, originalAmount, originalJournalId);

        // Act
        entry.Update(null, null, null); // Pass all nulls

        // Assert
        entry.DepreciationDate.Should().Be(originalDate);
        entry.Amount.Should().Be(originalAmount);
        entry.JournalEntryId.Should().Be(originalJournalId);
    }
}
