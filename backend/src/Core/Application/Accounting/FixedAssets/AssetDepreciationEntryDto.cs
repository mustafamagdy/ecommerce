using FSH.WebApi.Application.Common.Interfaces;
using System;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class AssetDepreciationEntryDto : IDto
{
    public Guid Id { get; set; }
    public Guid FixedAssetId { get; set; }
    // public string? FixedAssetName {get; set;} // Optional, if needed for display directly on entry DTO
    public DateTime DepreciationDate { get; set; }
    public decimal Amount { get; set; }
    public Guid? JournalEntryId { get; set; }
    public DateTime CreatedOn { get; set; }
}
