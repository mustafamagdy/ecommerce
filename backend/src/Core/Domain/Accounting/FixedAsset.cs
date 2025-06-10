using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public enum FixedAssetStatus
{
    UnderConstruction, // Or 'Pending', 'NotYetAcquired'
    Active,
    Inactive,
    InRepair,
    Disposed,
    Sold
}

public class FixedAsset : AuditableEntity, IAggregateRoot
{
    public string AssetNumber { get; private set; } = default!; // Unique identifier
    public string AssetName { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid AssetCategoryId { get; private set; }
    // public virtual AssetCategory AssetCategory { get; private set; } = default!; // Navigation
    public DateTime PurchaseDate { get; private set; }
    public decimal PurchaseCost { get; private set; }
    public decimal SalvageValue { get; private set; } // Estimated value at end of useful life
    public int UsefulLifeYears { get; private set; } // In years
    public Guid DepreciationMethodId { get; private set; }
    // public virtual DepreciationMethod DepreciationMethod { get; private set; } = default!; // Navigation
    public string? Location { get; private set; } // E.g., building, room, department
    public FixedAssetStatus Status { get; private set; }
    public DateTime? DisposalDate { get; private set; }
    public string? DisposalReason { get; private set; }
    public decimal? DisposalAmount { get; private set; }

    private readonly List<AssetDepreciationEntry> _depreciationEntries = new();
    public IReadOnlyCollection<AssetDepreciationEntry> DepreciationEntries => _depreciationEntries.AsReadOnly();

    public decimal AccumulatedDepreciation => _depreciationEntries.Sum(e => e.Amount);
    public decimal BookValue => PurchaseCost - AccumulatedDepreciation;


    // Private constructor for EF Core
    private FixedAsset() { }

    public FixedAsset(
        string assetNumber,
        string assetName,
        Guid assetCategoryId,
        DateTime purchaseDate,
        decimal purchaseCost,
        decimal salvageValue,
        int usefulLifeYears,
        Guid depreciationMethodId,
        string? description = null,
        string? location = null,
        FixedAssetStatus status = FixedAssetStatus.Active)
    {
        AssetNumber = assetNumber;
        AssetName = assetName;
        AssetCategoryId = assetCategoryId;
        PurchaseDate = purchaseDate;
        PurchaseCost = purchaseCost;

        if (salvageValue < 0)
            throw new ArgumentOutOfRangeException(nameof(salvageValue), "Salvage value cannot be negative.");
        if (purchaseCost < salvageValue)
            throw new ArgumentException("Purchase cost cannot be less than salvage value.");
        SalvageValue = salvageValue;

        if (usefulLifeYears <= 0)
            throw new ArgumentOutOfRangeException(nameof(usefulLifeYears), "Useful life must be positive.");
        UsefulLifeYears = usefulLifeYears;

        DepreciationMethodId = depreciationMethodId;
        Description = description;
        Location = location;
        Status = status;
    }

    public FixedAsset Update(
        string? assetName,
        string? description,
        Guid? assetCategoryId,
        DateTime? purchaseDate,
        decimal? purchaseCost,
        decimal? salvageValue,
        int? usefulLifeYears,
        Guid? depreciationMethodId,
        string? location,
        FixedAssetStatus? status,
        // Disposal fields are typically set via a specific Dispose action, not generic update
        DateTime? disposalDate = null,
        string? disposalReason = null,
        decimal? disposalAmount = null)
    {
        if (assetName is not null && AssetName?.Equals(assetName) is not true) AssetName = assetName;
        if (description is not null && Description?.Equals(description) is not true) Description = description;
        if (assetCategoryId.HasValue && AssetCategoryId != assetCategoryId.Value) AssetCategoryId = assetCategoryId.Value;
        if (purchaseDate.HasValue && PurchaseDate != purchaseDate.Value) PurchaseDate = purchaseDate.Value;

        // Validation for cost and salvage value updates
        decimal currentPurchaseCost = purchaseCost ?? PurchaseCost;
        decimal currentSalvageValue = salvageValue ?? SalvageValue;

        if (currentSalvageValue < 0)
            throw new ArgumentOutOfRangeException(nameof(salvageValue), "Salvage value cannot be negative.");
        if (currentPurchaseCost < currentSalvageValue)
            throw new ArgumentException("Purchase cost cannot be less than salvage value.");

        if (purchaseCost.HasValue) PurchaseCost = purchaseCost.Value;
        if (salvageValue.HasValue) SalvageValue = salvageValue.Value;


        if (usefulLifeYears.HasValue)
        {
            if (usefulLifeYears.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(usefulLifeYears), "Useful life must be positive.");
            UsefulLifeYears = usefulLifeYears.Value;
        }

        if (depreciationMethodId.HasValue && DepreciationMethodId != depreciationMethodId.Value) DepreciationMethodId = depreciationMethodId.Value;
        if (location is not null && Location?.Equals(location) is not true) Location = location;
        if (status.HasValue && Status != status.Value) Status = status.Value;
        // AssetNumber is typically not changed.

        // Update disposal fields only if explicitly provided (usually via Dispose method)
        if (disposalDate.HasValue) DisposalDate = disposalDate.Value;
        if (disposalReason is not null) DisposalReason = disposalReason;
        if (disposalAmount.HasValue) DisposalAmount = disposalAmount.Value;

        return this;
    }

    public void Dispose(DateTime disposalDate, string? reason, decimal? amount)
    {
        if (Status == FixedAssetStatus.Disposed && DisposalDate.HasValue)
        {
            // Potentially allow updating disposal details if already disposed.
            // Or throw new InvalidOperationException("Asset is already disposed.");
        }

        Status = FixedAssetStatus.Disposed;
        DisposalDate = disposalDate;
        DisposalReason = reason;
        DisposalAmount = amount;

        // Further logic: e.g., calculate gain/loss on disposal, zero out book value, etc.
        // This would likely involve a depreciation calculation up to disposalDate.
    }

    /// <summary>
    /// Calculates depreciation for a given period end date using Straight-Line method.
    /// Adds a new AssetDepreciationEntry if depreciation is applicable for the period.
    /// </summary>
    /// <param name="periodEndDate">The end date of the period for which to calculate depreciation.</param>
    /// <returns>The created AssetDepreciationEntry, or null if no depreciation was applicable for this period.</returns>
    public AssetDepreciationEntry? CalculateDepreciationForPeriod(DateTime periodEndDate)
    {
        if (Status != FixedAssetStatus.Active && Status != FixedAssetStatus.InRepair) // Only active/in-repair assets depreciate
            return null;

        if (periodEndDate < PurchaseDate) // Cannot depreciate before purchase
            return null;

        // Ensure periodEndDate is after the last depreciation date, if any.
        DateTime lastDepreciationDate = _depreciationEntries.Any()
            ? _depreciationEntries.Max(e => e.DepreciationDate)
            : PurchaseDate; // Start from purchase date if no prior depreciation

        if (periodEndDate <= lastDepreciationDate && _depreciationEntries.Any()) // If already depreciated for a period ending on or after this date
             return null;


        decimal currentAccumulatedDepreciation = AccumulatedDepreciation;
        decimal depreciableBase = PurchaseCost - SalvageValue;

        if (depreciableBase <= 0 || currentAccumulatedDepreciation >= depreciableBase) // Fully depreciated or no base to depreciate
            return null;

        // Straight-Line specific calculation
        // This assumes monthly depreciation. For more precision, daily or other frequencies might be needed.
        // Or, the DepreciationMethod entity could define how to calculate. For now, hardcoding SL.
        if (DepreciationMethod?.Name != "Straight-Line" && DepreciationMethodId != Guid.Empty /* Placeholder for actual SL method ID check */)
        {
            // For now, only implementing Straight-Line.
            // In a real system, this would use a strategy or data from DepreciationMethod.
            // throw new NotImplementedException("Only Straight-Line depreciation is currently supported.");
            // Or simply return null if the method is not SL, and handler can log/skip.
            Console.WriteLine($"Warning: Asset {AssetNumber} uses an unsupported depreciation method for calculation. Skipping.");
            return null;
        }

        decimal yearlyDepreciation = depreciableBase / UsefulLifeYears;
        decimal monthlyDepreciation = yearlyDepreciation / 12; // Simplified monthly

        // Determine how many full months have passed since the last depreciation or purchase date, up to periodEndDate.
        // This is a simplified calculation for monthly depreciation.
        // A more robust calculation would consider actual days in month/year or specific accounting period rules.

        // For simplicity, assume we are calculating for the month ending on periodEndDate.
        // And that this method is called once per month per asset.
        // The check "periodEndDate <= lastDepreciationDate" should prevent re-calculation for same period.

        decimal depreciationAmountForPeriod = monthlyDepreciation;

        // Adjust if the remaining depreciable amount is less than a full period's depreciation
        if (currentAccumulatedDepreciation + depreciationAmountForPeriod > depreciableBase)
        {
            depreciationAmountForPeriod = depreciableBase - currentAccumulatedDepreciation;
        }

        if (depreciationAmountForPeriod <= 0)
            return null;

        var newEntry = new AssetDepreciationEntry(this.Id, periodEndDate, depreciationAmountForPeriod);
        _depreciationEntries.Add(newEntry);
        // AccumulatedDepreciation and BookValue are recalculated by their getters.

        // Check if fully depreciated after this entry
        if (BookValue <= SalvageValue && Status == FixedAssetStatus.Active)
        {
            // Optional: Change status or log, but for now, just ensure no further depreciation.
            // This is implicitly handled by "currentAccumulatedDepreciation >= depreciableBase" check at start.
        }

        return newEntry;
    }
}
