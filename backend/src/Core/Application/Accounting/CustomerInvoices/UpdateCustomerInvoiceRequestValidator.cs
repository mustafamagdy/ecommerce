using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class UpdateCustomerInvoiceRequestValidator : CustomValidator<UpdateCustomerInvoiceRequest>
{
    public UpdateCustomerInvoiceRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmptyGuid();

        RuleFor(p => p.InvoiceDate)
            .NotEmpty()
            .When(p => p.InvoiceDate.HasValue);

        RuleFor(p => p.DueDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(p => p.InvoiceDate)
            .WithMessage("Due date must be on or after the invoice date.")
            .When(p => p.DueDate.HasValue && p.InvoiceDate.HasValue); // Only if both provided

        RuleFor(p => p.Currency)
            .Length(3)
            .When(p => !string.IsNullOrEmpty(p.Currency));

        RuleFor(p => p.Notes)
            .MaximumLength(2000)
            .When(p => p.Notes is not null); // Allow empty string

        RuleForEach(p => p.InvoiceItems)
            .SetValidator(new UpdateCustomerInvoiceItemRequestValidator())
            .When(p => p.InvoiceItems is not null); // Validate items only if list is provided
    }
}

public class UpdateCustomerInvoiceItemRequestValidator : CustomValidator<UpdateCustomerInvoiceItemRequest>
{
    public UpdateCustomerInvoiceItemRequestValidator()
    {
        // Id is optional: if null/empty, it's a new item for an existing invoice.
        // If provided, it's an existing item to update.
        RuleFor(i => i.Id)
            .NotEmptyGuid()
            .When(i => i.Id.HasValue && i.Id.Value != Guid.Empty);


        RuleFor(i => i.Description)
            .NotEmpty()
            .MaximumLength(1000)
            .When(i => i.Description is not null || !i.Id.HasValue); // Required if new, or if desc is explicitly being set

        RuleFor(i => i.ProductId)
            .NotEmptyGuid()
            .When(i => i.ProductId.HasValue);

        RuleFor(i => i.Quantity)
            .GreaterThan(0)
            .When(i => i.Quantity.HasValue);

        RuleFor(i => i.UnitPrice)
            .GreaterThan(0)
            .When(i => i.UnitPrice.HasValue);

        RuleFor(i => i.TaxAmount)
            .GreaterThanOrEqualTo(0)
            .When(i => i.TaxAmount.HasValue);
    }
}
