using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class UpdateVendorInvoiceRequestValidator : CustomValidator<UpdateVendorInvoiceRequest>
{
    public UpdateVendorInvoiceRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty(); // Assuming NotEmptyGuid or similar

        RuleFor(p => p.SupplierId)
            .NotEmpty() // Or NotEmptyGuid
            .When(p => p.SupplierId.HasValue);

        RuleFor(p => p.InvoiceNumber)
            .NotEmpty()
            .MaximumLength(100)
            .When(p => !string.IsNullOrEmpty(p.InvoiceNumber));

        RuleFor(p => p.InvoiceDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Invoice date must be today or in the past, or not too far in the future.")
            .When(p => p.InvoiceDate.HasValue);

        RuleFor(p => p.DueDate)
            .GreaterThanOrEqualTo(p => p.InvoiceDate)
            .WithMessage("Due date must be on or after the invoice date.")
            .When(p => p.DueDate.HasValue && p.InvoiceDate.HasValue); // Only if both dates are present

        RuleFor(p => p.TotalAmount)
            .GreaterThan(0)
            .When(p => p.TotalAmount.HasValue);

        RuleFor(p => p.Currency)
            .Length(3)
            .When(p => !string.IsNullOrEmpty(p.Currency));

        RuleFor(p => p.Notes)
            .MaximumLength(2000)
            .When(p => p.Notes is not null);

        // Validate items if the list itself is provided (not null)
        RuleForEach(p => p.InvoiceItems)
            .SetValidator(new UpdateVendorInvoiceItemRequestValidator())
            .When(p => p.InvoiceItems is not null);

        // Complex validation: if items are provided, their sum should match TotalAmount if TotalAmount is also provided
        RuleFor(p => p)
            .Must(p => p.InvoiceItems!.Sum(item =>
                (item.Quantity ?? 0) * (item.UnitPrice ?? 0) // Use 0 if quantity or unitprice is null for this check
            ) == p.TotalAmount.Value)
            .WithMessage("If InvoiceItems and TotalAmount are both provided, the sum of item totals (Quantity * UnitPrice) must equal the invoice TotalAmount.")
            .When(p => p.InvoiceItems != null && p.InvoiceItems.Any() && p.TotalAmount.HasValue &&
                         p.InvoiceItems.All(i => i.Quantity.HasValue && i.UnitPrice.HasValue)); // Only when all items have Qty and Price

         RuleFor(p => p)
            .Must(p => p.InvoiceItems!.Sum(item => item.TotalAmount ?? 0) == p.TotalAmount.Value)
            .WithMessage("If InvoiceItems and TotalAmount are both provided, the sum of item TotalAmounts must equal the invoice TotalAmount.")
            .When(p => p.InvoiceItems != null && p.InvoiceItems.Any() && p.TotalAmount.HasValue &&
                         p.InvoiceItems.All(i => i.TotalAmount.HasValue));

    }
}

public class UpdateVendorInvoiceItemRequestValidator : CustomValidator<UpdateVendorInvoiceItemRequest>
{
    public UpdateVendorInvoiceItemRequestValidator()
    {
        // For existing items, Id must be present. For new items, Id is null.
        // RuleFor(i => i.Id)
        //     .NotEmptyGuid()
        //     .When(i => i.Id.HasValue && i.Id.Value != Guid.Empty); // If Id is given, it shouldn't be empty Guid

        RuleFor(i => i.Description)
            .NotEmpty()
            .MaximumLength(1000)
            .When(i => i.Description is not null || !i.Id.HasValue); // Required if new, or if description is explicitly being set

        RuleFor(i => i.Quantity)
            .GreaterThan((decimal)0.00009)
            .When(i => i.Quantity.HasValue);

        RuleFor(i => i.UnitPrice)
            .GreaterThan((decimal)0.009)
            .When(i => i.UnitPrice.HasValue);

        RuleFor(i => i.TotalAmount)
            .GreaterThan((decimal)0.009)
            .When(i => i.TotalAmount.HasValue);

        // If quantity and unitprice are both set, then total amount must match
        RuleFor(item => item.TotalAmount)
            .Must((item, total) => total == item.Quantity * item.UnitPrice)
            .WithMessage("Item TotalAmount must be equal to Quantity * UnitPrice if both Quantity and UnitPrice are provided.")
            .When(item => item.Quantity.HasValue && item.UnitPrice.HasValue && item.TotalAmount.HasValue);


        RuleFor(i => i.TaxAmount)
            .GreaterThanOrEqualTo(0)
            .When(i => i.TaxAmount.HasValue);
    }
}
