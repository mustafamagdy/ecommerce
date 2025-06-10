using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.PaymentTerms;
using System;

namespace Application.Tests.Accounting.PaymentTerms;

public class CreatePaymentTermRequestValidatorTests
{
    private readonly CreatePaymentTermRequestValidator _validator = new();

    private CreatePaymentTermRequest CreateValidRequest() => new()
    {
        Name = "Net 30",
        Description = "Payment due in 30 days",
        DaysUntilDue = 30,
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
    public void Should_Have_Error_When_DaysUntilDue_Is_Negative()
    {
        var request = CreateValidRequest();
        request.DaysUntilDue = -1;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DaysUntilDue)
            .WithErrorMessage("Days until due must be zero or positive.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_DaysUntilDue_Is_Zero()
    {
        var request = CreateValidRequest();
        request.DaysUntilDue = 0;
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.DaysUntilDue);
    }

    [Fact]
    public void Should_Not_Have_Error_When_OptionalFields_Are_Null()
    {
        var request = new CreatePaymentTermRequest
        {
            Name = "Net C.O.D.",
            DaysUntilDue = 0,
            Description = null, // Optional
            IsActive = true
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
