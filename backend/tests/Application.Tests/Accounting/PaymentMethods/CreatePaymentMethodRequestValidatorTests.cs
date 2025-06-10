using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.PaymentMethods;
using System;

namespace Application.Tests.Accounting.PaymentMethods;

public class CreatePaymentMethodRequestValidatorTests
{
    private readonly CreatePaymentMethodRequestValidator _validator = new();

    private CreatePaymentMethodRequest CreateValidRequest() => new()
    {
        Name = "Credit Card",
        Description = "Payment via credit card network",
        IsActive = true
    };

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_NullOrEmpty()
    {
        var request = CreateValidRequest();
        request.Name = null!;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);

        request.Name = string.Empty;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        var request = CreateValidRequest();
        request.Name = new string('A', 101);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_MaxLength()
    {
        var request = CreateValidRequest();
        request.Description = new string('A', 257);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Description_Is_Null()
    {
        var request = CreateValidRequest();
        request.Description = null;
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
