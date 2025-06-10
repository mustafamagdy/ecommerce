using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid
using System.Linq; // For Sum

namespace FSH.WebApi.Application.Accounting.CustomerPayments;

public class CreateCustomerPaymentRequestValidator : CustomValidator<CreateCustomerPaymentRequest>
{
    public CreateCustomerPaymentRequestValidator()
    {
        RuleFor(p => p.CustomerId)
            .NotEmptyGuid();

        RuleFor(p => p.PaymentDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Payment date must be today, in the past, or not too far in the future.");

        RuleFor(p => p.AmountReceived)
            .GreaterThan(0);

        RuleFor(p => p.PaymentMethodId)
            .NotEmptyGuid();

        RuleFor(p => p.ReferenceNumber)
            .MaximumLength(256);

        RuleFor(p => p.Notes)
            .MaximumLength(1024);

        RuleForEach(p => p.Applications)
            .SetValidator(new CustomerPaymentApplicationRequestItemValidator());

        // Business rule: Sum of amounts applied must not exceed the total amount received.
        // It can be less than (unapplied amount) or equal.
        RuleFor(p => p)
            .Must(p => p.Applications.Sum(a => a.AmountApplied) <= p.AmountReceived)
            .WithMessage("The sum of all applied amounts cannot exceed the Amount Received.")
            .When(p => p.Applications != null && p.Applications.Any());

        // Optional: If applications are present, sum must equal AmountReceived (no partial unapplied at creation time)
        // RuleFor(p => p)
        //     .Must(p => p.Applications.Sum(a => a.AmountApplied) == p.AmountReceived)
        //     .WithMessage("The sum of all applied amounts must equal the Amount Received when applications are provided.")
        //     .When(p => p.Applications != null && p.Applications.Any());
    }
}

public class CustomerPaymentApplicationRequestItemValidator : CustomValidator<CustomerPaymentApplicationRequestItem>
{
    public CustomerPaymentApplicationRequestItemValidator()
    {
        RuleFor(item => item.CustomerInvoiceId)
            .NotEmptyGuid();

        RuleFor(item => item.AmountApplied)
            .GreaterThan(0);
    }
}
