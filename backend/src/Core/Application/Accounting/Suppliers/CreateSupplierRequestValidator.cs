using FluentValidation;
using FSH.WebApi.Application.Common.Validation;

namespace FSH.WebApi.Application.Accounting.Suppliers;

public class CreateSupplierRequestValidator : CustomValidator<CreateSupplierRequest>
{
    public CreateSupplierRequestValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(256)
                .WithMessage("Supplier Name must not exceed 256 characters.");

        RuleFor(p => p.ContactInfo)
            .MaximumLength(256)
                .WithMessage("Contact Information must not exceed 256 characters.");

        RuleFor(p => p.Address)
            .MaximumLength(1024)
                .WithMessage("Address must not exceed 1024 characters.");

        RuleFor(p => p.TaxId)
            .MaximumLength(50)
                .WithMessage("Tax ID must not exceed 50 characters.");

        RuleFor(p => p.BankDetails)
            .MaximumLength(1024)
                .WithMessage("Bank Details must not exceed 1024 characters.");

        // RuleFor(p => p.DefaultPaymentTermId)
        //     .NotEmptyGuid() // Assuming NotEmptyGuid is a custom validator for Guids if required
        //     .When(p => p.DefaultPaymentTermId.HasValue);
        // No specific validation for DefaultPaymentTermId here, can be added if it must exist in DB
    }
}
