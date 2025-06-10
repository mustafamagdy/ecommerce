using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.VendorPayments;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Tests.Accounting.VendorPayments;

public class CreateVendorPaymentRequestValidatorTests
{
    private readonly CreateVendorPaymentRequestValidator _validator = new();

    private CreateVendorPaymentRequest CreateValidRequest(decimal amountPaid = 150m, List<VendorPaymentApplicationRequestItem>? applications = null)
    {
        if (applications == null)
        {
            applications = new List<VendorPaymentApplicationRequestItem>
            {
                new VendorPaymentApplicationRequestItem { VendorInvoiceId = Guid.NewGuid(), AmountApplied = 100m },
                new VendorPaymentApplicationRequestItem { VendorInvoiceId = Guid.NewGuid(), AmountApplied = 50m }
            };
        }

        return new CreateVendorPaymentRequest
        {
            SupplierId = Guid.NewGuid(),
            PaymentDate = DateTime.UtcNow.Date,
            AmountPaid = amountPaid,
            PaymentMethodId = Guid.NewGuid(),
            ReferenceNumber = "REF-VALID-001",
            Notes = "Valid payment notes",
            Applications = applications
        };
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_SupplierId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.SupplierId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SupplierId).WithErrorMessage("'Supplier Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_PaymentDate_Is_Default()
    {
        var request = CreateValidRequest();
        request.PaymentDate = default;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PaymentDate);
    }

    [Fact]
    public void Should_Have_Error_When_AmountPaid_Is_ZeroOrNegative()
    {
        var request = CreateValidRequest(amountPaid: 0);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AmountPaid);

        request = CreateValidRequest(amountPaid: -100);
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AmountPaid);
    }

    [Fact]
    public void Should_Have_Error_When_PaymentMethodId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.PaymentMethodId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PaymentMethodId).WithErrorMessage("'Payment Method Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_Applications_List_Is_Empty()
    {
        var request = CreateValidRequest(applications: new List<VendorPaymentApplicationRequestItem>());
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Applications).WithErrorMessage("At least one invoice application is required.");
    }

    [Fact]
    public void Should_Have_Error_For_Nested_Application_When_VendorInvoiceId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.Applications[0].VendorInvoiceId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Applications[0].VendorInvoiceId");
    }

    [Fact]
    public void Should_Have_Error_For_Nested_Application_When_AmountApplied_Is_Zero()
    {
        var request = CreateValidRequest();
        request.Applications[0].AmountApplied = 0;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Applications[0].AmountApplied");
    }

    [Fact]
    public void Should_Have_Error_When_Sum_Of_Applications_AmountApplied_Not_Equal_To_AmountPaid()
    {
        var request = CreateValidRequest(amountPaid: 200m); // Total applied is 150m from default helper
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x) // Error for the root object
            .WithErrorMessage("The sum of all applied amounts must equal the Amount Paid.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Sum_Of_Applications_AmountApplied_Equals_AmountPaid()
    {
        var request = CreateValidRequest(amountPaid: 150m, new List<VendorPaymentApplicationRequestItem>
        {
            new VendorPaymentApplicationRequestItem { VendorInvoiceId = Guid.NewGuid(), AmountApplied = 70m },
            new VendorPaymentApplicationRequestItem { VendorInvoiceId = Guid.NewGuid(), AmountApplied = 80m }
        }); // Sum is 150m
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
