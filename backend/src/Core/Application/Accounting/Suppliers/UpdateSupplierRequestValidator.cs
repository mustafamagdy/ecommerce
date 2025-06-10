using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // Assuming this contains NotEmptyGuid
using System;

namespace FSH.WebApi.Application.Accounting.Suppliers;

public class UpdateSupplierRequestValidator : CustomValidator<UpdateSupplierRequest>
{
    public UpdateSupplierRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmptyGuid(); // Assumes NotEmptyGuid is available for validating Guids.

        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(256)
                .WithMessage("Supplier Name must not exceed 256 characters.")
            .When(p => p.Name is not null); // Apply only if Name is provided

        RuleFor(p => p.ContactInfo)
            .MaximumLength(256)
                .WithMessage("Contact Information must not exceed 256 characters.")
            .When(p => p.ContactInfo is not null);

        RuleFor(p => p.Address)
            .MaximumLength(1024)
                .WithMessage("Address must not exceed 1024 characters.")
            .When(p => p.Address is not null);

        RuleFor(p => p.TaxId)
            .MaximumLength(50)
                .WithMessage("Tax ID must not exceed 50 characters.")
            .When(p => p.TaxId is not null);

        RuleFor(p => p.BankDetails)
            .MaximumLength(1024)
                .WithMessage("Bank Details must not exceed 1024 characters.")
            .When(p => p.BankDetails is not null);

        // No specific validation for DefaultPaymentTermId here for now.
        // If it were required that if a Guid is provided it must be a valid existing PaymentTermId,
        // that would typically be a database check done in the handler.
        // RuleFor(p => p.DefaultPaymentTermId)
        //     .NotEmptyGuid()
        //     .When(p => p.DefaultPaymentTermId.HasValue);
    }
}
