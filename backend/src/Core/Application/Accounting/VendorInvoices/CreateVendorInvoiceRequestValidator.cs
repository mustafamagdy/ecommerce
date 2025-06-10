using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid if used
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class CreateVendorInvoiceRequestValidator : CustomValidator<CreateVendorInvoiceRequest>
{
    public CreateVendorInvoiceRequestValidator()
    {
        RuleFor(p => p.SupplierId)
            .NotEmpty(); // Assuming NotEmptyGuid or similar if Guid specific check needed

        RuleFor(p => p.InvoiceNumber)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.InvoiceDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)) // Invoice date cannot be too far in future
            .WithMessage("Invoice date must be today or in the past, or not too far in the future.");


        RuleFor(p => p.DueDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(p => p.InvoiceDate)
            .WithMessage("Due date must be on or after the invoice date.");

        RuleFor(p => p.TotalAmount)
            .GreaterThan(0);

        RuleFor(p => p.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(p => p.Notes)
            .MaximumLength(2000);

        RuleFor(p => p.InvoiceItems)
            .NotEmpty()
            .WithMessage("Invoice must have at least one item.");

        RuleForEach(p => p.InvoiceItems).SetValidator(new CreateVendorInvoiceItemRequestValidator());

        // Validate that the sum of item TotalAmounts equals the invoice TotalAmount
        RuleFor(p => p)
            .Must(p => p.InvoiceItems.Sum(item => item.Quantity * item.UnitPrice) == p.TotalAmount)
            .WithMessage("The sum of item totals (Quantity * UnitPrice) must equal the invoice TotalAmount.")
            .When(p => p.InvoiceItems != null && p.InvoiceItems.Any());

        // Validate that the sum of item TotalAmounts (as provided) equals the invoice TotalAmount
        // This is if item.TotalAmount is pre-calculated and sent from client
        RuleFor(p => p)
            .Must(p => p.InvoiceItems.Sum(item => item.TotalAmount) == p.TotalAmount)
            .WithMessage("The sum of item TotalAmounts must equal the invoice TotalAmount.")
            .When(p => p.InvoiceItems != null && p.InvoiceItems.Any() && p.InvoiceItems.All(i => i.TotalAmount > 0));


    }
}

public class CreateVendorInvoiceItemRequestValidator : CustomValidator<CreateVendorInvoiceItemRequest>
{
    public CreateVendorInvoiceItemRequestValidator()
    {
        RuleFor(i => i.Description)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(i => i.Quantity)
            .GreaterThan((decimal)0.00009); // Check for positive quantity

        RuleFor(i => i.UnitPrice)
            .GreaterThan((decimal)0.009); // Check for positive unit price

        RuleFor(i => i.TotalAmount)
            .GreaterThan((decimal)0.009)
            .Must((item, total) => total == item.Quantity * item.UnitPrice)
            .WithMessage("Item TotalAmount must be equal to Quantity * UnitPrice.")
            .When(item => item.Quantity > 0 && item.UnitPrice > 0);


        RuleFor(i => i.TaxAmount)
            .GreaterThanOrEqualTo(0);
    }
}
