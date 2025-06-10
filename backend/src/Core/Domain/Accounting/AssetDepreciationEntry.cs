using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public class AssetDepreciationEntry : AuditableEntity
{
    public Guid FixedAssetId { get; private set; }
    // public virtual FixedAsset FixedAsset { get; private set; } = default!; // Navigation property

    public DateTime DepreciationDate { get; private set; } // Or PeriodEndDate
    public decimal Amount { get; private set; } // Depreciation amount for this period
    public Guid? JournalEntryId { get; private set; } // For future GL integration

    // Private constructor for EF Core
    private AssetDepreciationEntry() { }

    public AssetDepreciationEntry(
        Guid fixedAssetId,
        DateTime depreciationDate,
        decimal amount,
        Guid? journalEntryId = null)
    {
        FixedAssetId = fixedAssetId;
        DepreciationDate = depreciationDate;

        if (amount < 0) // Amount can be zero for final adjustment, but not negative.
            throw new ArgumentOutOfRangeException(nameof(amount), "Depreciation amount cannot be negative.");
        Amount = amount;
        JournalEntryId = journalEntryId;
    }

    public void Update(DateTime? depreciationDate, decimal? amount, Guid? journalEntryId)
    {
        // Generally, depreciation entries are immutable once created.
        // If adjustments are needed, reversing entries and new entries are preferred.
        // However, if direct updates are allowed by business rules:
        if (depreciationDate.HasValue) DepreciationDate = depreciationDate.Value;
        if (amount.HasValue)
        {
            if (amount.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Depreciation amount cannot be negative.");
            Amount = amount.Value;
        }
        if (journalEntryId.HasValue) JournalEntryId = journalEntryId.Value;
    }
}
