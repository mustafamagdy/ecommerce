using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid
using System.Linq; // For Sum

namespace FSH.WebApi.Application.Accounting.VendorPayments;

public class CreateVendorPaymentRequestValidator : CustomValidator<CreateVendorPaymentRequest>
{
    public CreateVendorPaymentRequestValidator()
    {
        RuleFor(p => p.SupplierId)
            .NotEmptyGuid();

        RuleFor(p => p.PaymentDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)) // Payment date cannot be too far in future
            .WithMessage("Payment date must be today, in the past, or not too far in the future.");

        RuleFor(p => p.AmountPaid)
            .GreaterThan(0);

        RuleFor(p => p.PaymentMethodId)
            .NotEmptyGuid();

        RuleFor(p => p.ReferenceNumber)
            .MaximumLength(256);

        RuleFor(p => p.Notes)
            .MaximumLength(1024);

        RuleFor(p => p.Applications)
            .NotEmpty()
            .WithMessage("At least one invoice application is required.");

        RuleForEach(p => p.Applications)
            .SetValidator(new VendorPaymentApplicationRequestItemValidator());

        // Business rule: Sum of amounts applied should equal the total amount paid.
        // This rule can be complex depending on partial application allowances at creation.
        // If AmountPaid is authoritative and applications are just distribution:
        RuleFor(p => p)
            .Must(p => p.Applications.Sum(a => a.AmountApplied) == p.AmountPaid)
            .WithMessage("The sum of all applied amounts must equal the Amount Paid.")
            .When(p => p.Applications != null && p.Applications.Any());
    }
}

public class VendorPaymentApplicationRequestItemValidator : CustomValidator<VendorPaymentApplicationRequestItem>
{
    public VendorPaymentApplicationRequestItemValidator()
    {
        RuleFor(item => item.VendorInvoiceId)
            .NotEmptyGuid();

        RuleFor(item => item.AmountApplied)
            .GreaterThan(0);
    }
}
