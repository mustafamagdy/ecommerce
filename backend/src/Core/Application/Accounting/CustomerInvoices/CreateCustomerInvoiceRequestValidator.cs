using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class CreateCustomerInvoiceRequestValidator : CustomValidator<CreateCustomerInvoiceRequest>
{
    public CreateCustomerInvoiceRequestValidator()
    {
        RuleFor(p => p.CustomerId)
            .NotEmptyGuid();

        RuleFor(p => p.OrderId)
            .NotEmptyGuid()
            .When(p => p.OrderId.HasValue);

        RuleFor(p => p.InvoiceDate)
            .NotEmpty();

        RuleFor(p => p.DueDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(p => p.InvoiceDate)
            .WithMessage("Due date must be on or after the invoice date.");

        RuleFor(p => p.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(p => p.Notes)
            .MaximumLength(2000);

        RuleFor(p => p.InvoiceItems)
            .NotEmpty()
            .WithMessage("Invoice must have at least one item.");

        RuleForEach(p => p.InvoiceItems).SetValidator(new CreateCustomerInvoiceItemRequestValidator());
    }
}

public class CreateCustomerInvoiceItemRequestValidator : CustomValidator<CreateCustomerInvoiceItemRequest>
{
    public CreateCustomerInvoiceItemRequestValidator()
    {
        RuleFor(i => i.Description)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(i => i.ProductId)
            .NotEmptyGuid()
            .When(i => i.ProductId.HasValue);

        RuleFor(i => i.Quantity)
            .GreaterThan(0);

        RuleFor(i => i.UnitPrice)
            .GreaterThan(0);

        RuleFor(i => i.TaxAmount)
            .GreaterThanOrEqualTo(0);
    }
}
