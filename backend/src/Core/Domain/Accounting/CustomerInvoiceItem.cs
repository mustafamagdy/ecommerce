using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.Catalog; // Assuming Product entity is in this namespace
using System;

namespace FSH.WebApi.Domain.Accounting;

public class CustomerInvoiceItem : AuditableEntity
{
    public Guid CustomerInvoiceId { get; private set; }
    // public virtual CustomerInvoice CustomerInvoice { get; private set; } = default!; // Navigation

    public string Description { get; private set; } = default!;
    public Guid? ProductId { get; private set; }
    // public virtual Product? Product { get; private set; } // Navigation

    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalAmount { get; private set; } // Quantity * UnitPrice
    public decimal TaxAmount { get; private set; }   // Tax for this specific item, included in TotalAmount or separate?
                                                  // Assuming TotalAmount = (Quantity * UnitPrice) + TaxAmount for simplicity here.
                                                  // Or, TotalAmount is pre-tax, and invoice level total includes sum of TaxAmounts.
                                                  // For now: TotalAmount = Quantity * UnitPrice, and TaxAmount is separate.

    // Private constructor for EF Core
    private CustomerInvoiceItem() { }

    public CustomerInvoiceItem(
        Guid customerInvoiceId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxAmount, // If tax is per item
        Guid? productId = null)
    {
        CustomerInvoiceId = customerInvoiceId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        ProductId = productId;
        TaxAmount = taxAmount; // Store tax per item
        TotalAmount = Quantity * UnitPrice; // Line total before tax, or after tax? Let's assume pre-tax line total.
                                            // The main CustomerInvoice.TotalAmount will sum these line totals + their taxes.
                                            // This needs to be consistent.
                                            // Revisiting: CustomerInvoice.TotalAmount should be SUM(Item.TotalAmount + Item.TaxAmount)
                                            // OR Item.TotalAmount = (Qty*Price) and CustomerInvoice.TotalAmount = SUM(Item.TotalAmount) + SUM(Item.TaxAmount)
                                            // Let's make Item.TotalAmount = Qty*Price (exclusive of tax for this item line)
                                            // And CustomerInvoice.TotalAmount will be sum of (Item.TotalAmount + Item.TaxAmount)
                                            // This means RecalculateTotalAmount in CustomerInvoice needs to be sum(item.TotalAmount + item.TaxAmount)
    }


    public void Update(string? description, decimal? quantity, decimal? unitPrice, decimal? taxAmount, Guid? productId)
    {
        if (description is not null && Description != description) Description = description;
        if (productId.HasValue && ProductId != productId.Value) ProductId = productId.Value;
        else if (productId == Guid.Empty && ProductId != null) ProductId = null;


        bool quantityOrPriceChanged = false;
        if (quantity.HasValue && Quantity != quantity.Value)
        {
            Quantity = quantity.Value;
            quantityOrPriceChanged = true;
        }
        if (unitPrice.HasValue && UnitPrice != unitPrice.Value)
        {
            UnitPrice = unitPrice.Value;
            quantityOrPriceChanged = true;
        }

        if (quantityOrPriceChanged)
        {
            TotalAmount = Quantity * UnitPrice; // Recalculate if quantity or unit price changes
        }

        if (taxAmount.HasValue && TaxAmount != taxAmount.Value) TaxAmount = taxAmount.Value;
    }
}
