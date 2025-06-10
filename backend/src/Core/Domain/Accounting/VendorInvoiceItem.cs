using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.Catalog; // Assuming Product entity is in this namespace
using System;

namespace FSH.WebApi.Domain.Accounting;

public class VendorInvoiceItem : AuditableEntity
{
    public Guid VendorInvoiceId { get; private set; }
    public virtual VendorInvoice VendorInvoice { get; private set; } = default!; // Navigation property

    public string Description { get; private set; } = default!;
    public Guid? ProductId { get; private set; }
    public virtual Product? Product { get; private set; } // Navigation property, assuming Product entity exists

    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalAmount { get; private set; } // Should be Quantity * UnitPrice
    public decimal TaxAmount { get; private set; } // Tax for this specific item

    // It's good practice to have a constructor to ensure required fields are set
    public VendorInvoiceItem(
        Guid vendorInvoiceId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxAmount,
        Guid? productId = null)
    {
        VendorInvoiceId = vendorInvoiceId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalAmount = quantity * unitPrice; // Calculated property
        TaxAmount = taxAmount;
        ProductId = productId;
    }

    public VendorInvoiceItem Update(
        string? description,
        decimal? quantity,
        decimal? unitPrice,
        decimal? taxAmount,
        Guid? productId)
    {
        if (description is not null && Description?.Equals(description) is not true) Description = description;
        if (productId.HasValue && ProductId != productId.Value) ProductId = productId.Value; // Note: allow unsetting ProductId by passing null
        else if (productId == Guid.Empty && ProductId != null) ProductId = null; // Convention to unset if Guid.Empty is passed

        bool quantityOrPriceUpdated = false;
        if (quantity.HasValue && Quantity != quantity.Value)
        {
            Quantity = quantity.Value;
            quantityOrPriceUpdated = true;
        }
        if (unitPrice.HasValue && UnitPrice != unitPrice.Value)
        {
            UnitPrice = unitPrice.Value;
            quantityOrPriceUpdated = true;
        }

        if (quantityOrPriceUpdated)
        {
            TotalAmount = Quantity * UnitPrice; // Recalculate if quantity or unit price changes
        }

        if (taxAmount.HasValue && TaxAmount != taxAmount.Value) TaxAmount = taxAmount.Value;

        return this;
    }

    // If ProductId is set, this method could be used to associate the actual Product entity
    public void SetProduct(Product product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        if (ProductId != product.Id)
        {
            // This might be an issue if ProductId was already set to something else.
            // Decide on the business logic: throw error, or allow change.
            // For now, let's assume ProductId should match.
            if (ProductId.HasValue && ProductId != product.Id)
            {
                // Consider logging a warning or throwing an exception if ProductId was already set to a different product.
            }
            ProductId = product.Id;
        }
        Product = product;
    }
}
