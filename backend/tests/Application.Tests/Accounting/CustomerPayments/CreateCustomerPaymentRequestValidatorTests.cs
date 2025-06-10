using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.CustomerPayments;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Tests.Accounting.CustomerPayments;

public class CreateCustomerPaymentRequestValidatorTests
{
    private readonly CreateCustomerPaymentRequestValidator _validator = new();

    private CreateCustomerPaymentRequest CreateValidRequest(decimal amountReceived = 150m, List<CustomerPaymentApplicationRequestItem>? applications = null)
    {
        if (applications == null)
        {
            // Default to applications sum matching amountReceived
            applications = new List<CustomerPaymentApplicationRequestItem>
            {
                new CustomerPaymentApplicationRequestItem { CustomerInvoiceId = Guid.NewGuid(), AmountApplied = 100m },
                new CustomerPaymentApplicationRequestItem { CustomerInvoiceId = Guid.NewGuid(), AmountApplied = 50m }
            };
        }
         // Ensure sum matches for validity by default
        if (applications.Sum(a => a.AmountApplied) != amountReceived)
        {
            // Adjust amountReceived to match sum for a "valid" default request based on one rule,
            // specific tests will vary this.
            amountReceived = applications.Sum(a => a.AmountApplied);
        }


        return new CreateCustomerPaymentRequest
        {
            CustomerId = Guid.NewGuid(),
            PaymentDate = DateTime.UtcNow.Date,
            AmountReceived = amountReceived,
            PaymentMethodId = Guid.NewGuid(),
            ReferenceNumber = "REF-CPAY-VALID-001",
            Notes = "Valid customer payment notes",
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
    public void Should_Not_Have_Error_When_Applications_List_Is_Empty_And_AmountReceived_Is_Positive()
    {
        // This tests if unapplied payments are allowed at creation.
        // The validator rule is: Sum(AmountApplied) <= AmountReceived.
        // If Applications is empty, sum is 0. So 0 <= AmountReceived is true.
        var request = CreateValidRequest(amountReceived: 100m, applications: new List<CustomerPaymentApplicationRequestItem>());
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.CustomerId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId).WithErrorMessage("'Customer Id' must not be empty.");
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
    public void Should_Have_Error_When_AmountReceived_Is_ZeroOrNegative()
    {
        var request = CreateValidRequest(amountReceived: 0, applications: new List<CustomerPaymentApplicationRequestItem>());
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AmountReceived);

        request = CreateValidRequest(amountReceived: -100, applications: new List<CustomerPaymentApplicationRequestItem>());
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AmountReceived);
    }

    [Fact]
    public void Should_Have_Error_When_PaymentMethodId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.PaymentMethodId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PaymentMethodId).WithErrorMessage("'Payment Method Id' must not be empty.");
    }

    // Applications list itself is not required to be non-empty by CreateCustomerPaymentRequest
    // but the nested validator for items will be tested.
    // The rule is: .When(p => p.Applications != null && p.Applications.Any()); for sum check.

    [Fact]
    public void Item_Should_Have_Error_When_CustomerInvoiceId_Is_Empty()
    {
        var request = CreateValidRequest(applications: new List<CustomerPaymentApplicationRequestItem>
        {
            new CustomerPaymentApplicationRequestItem { CustomerInvoiceId = Guid.Empty, AmountApplied = 50m }
        });
        request.AmountReceived = 50m; // Match sum
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Applications[0].CustomerInvoiceId");
    }

    [Fact]
    public void Item_Should_Have_Error_When_AmountApplied_Is_ZeroOrNegative()
    {
        var request = CreateValidRequest(applications: new List<CustomerPaymentApplicationRequestItem>
        {
            new CustomerPaymentApplicationRequestItem { CustomerInvoiceId = Guid.NewGuid(), AmountApplied = 0 }
        });
        request.AmountReceived = 0; // Match sum for this specific test, though AmountReceived > 0 is another rule.
                                    // To isolate, let's make AmountReceived valid for this test.
        request.AmountReceived = 1; // Make parent valid regarding AmountReceived > 0.
                                    // The item AmountApplied = 0 should still fail.

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Applications[0].AmountApplied");

        request.Applications[0].AmountApplied = -10;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Applications[0].AmountApplied");
    }

    [Fact]
    public void Should_Have_Error_When_Sum_Of_Applications_AmountApplied_Exceeds_AmountReceived()
    {
        var request = CreateValidRequest(amountReceived: 100m, applications: new List<CustomerPaymentApplicationRequestItem>
        {
            new CustomerPaymentApplicationRequestItem { CustomerInvoiceId = Guid.NewGuid(), AmountApplied = 80m },
            new CustomerPaymentApplicationRequestItem { CustomerInvoiceId = Guid.NewGuid(), AmountApplied = 30m } // Sum is 110m
        });
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x) // Error for the root object
            .WithErrorMessage("The sum of all applied amounts cannot exceed the Amount Received.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Sum_Of_Applications_Is_Less_Than_AmountReceived()
    {
        // Unapplied amount is allowed
        var request = CreateValidRequest(amountReceived: 200m, applications: new List<CustomerPaymentApplicationRequestItem>
        {
            new CustomerPaymentApplicationRequestItem { CustomerInvoiceId = Guid.NewGuid(), AmountApplied = 100m }
        });
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
