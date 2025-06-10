using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.PaymentMethods;
using System;

namespace Application.Tests.Accounting.PaymentMethods;

public class UpdatePaymentMethodRequestValidatorTests
{
    private readonly UpdatePaymentMethodRequestValidator _validator = new();

    private UpdatePaymentMethodRequest CreateValidRequest(Guid? id = null, string? name = "Credit Card Updated") => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = name,
        Description = "Updated payment method description",
        IsActive = false
    };

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid_With_All_Fields()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Only_Id_And_One_Field_Is_Provided()
    {
        var request = new UpdatePaymentMethodRequest { Id = Guid.NewGuid(), Name = "Only Name Changed" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();

        request = new UpdatePaymentMethodRequest { Id = Guid.NewGuid(), IsActive = true };
        result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var request = CreateValidRequest(id: Guid.Empty);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage("'Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty_If_Provided()
    {
        var request = CreateValidRequest(name: string.Empty);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Null_Not_Provided()
    {
        var request = new UpdatePaymentMethodRequest { Id = Guid.NewGuid(), Description = "Test" }; // Name is null
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength_If_Provided()
    {
        var request = CreateValidRequest(name: new string('A', 101));
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_MaxLength_If_Provided()
    {
        var request = CreateValidRequest();
        request.Description = new string('A', 257);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Description_Is_Null_If_Provided()
    {
        // This means explicitly setting it to null, which is allowed for optional fields.
        var request = new UpdatePaymentMethodRequest { Id = Guid.NewGuid(), Description = null };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
