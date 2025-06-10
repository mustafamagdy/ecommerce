using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.PaymentTerms;
using System;

namespace Application.Tests.Accounting.PaymentTerms;

public class UpdatePaymentTermRequestValidatorTests
{
    private readonly UpdatePaymentTermRequestValidator _validator = new();

    private UpdatePaymentTermRequest CreateValidRequest(Guid? id = null, string? name = "Net 30 Updated") => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = name,
        Description = "Updated payment term",
        DaysUntilDue = 35,
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
        var request = new UpdatePaymentTermRequest { Id = Guid.NewGuid(), Name = "Only Name Changed" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();

        request = new UpdatePaymentTermRequest { Id = Guid.NewGuid(), DaysUntilDue = 10 };
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
        var request = new UpdatePaymentTermRequest { Id = Guid.NewGuid(), DaysUntilDue = 15 }; // Name is null
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
    public void Should_Have_Error_When_DaysUntilDue_Is_Negative_If_Provided()
    {
        var request = CreateValidRequest();
        request.DaysUntilDue = -5;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DaysUntilDue)
            .WithErrorMessage("Days until due must be zero or positive.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_DaysUntilDue_Is_Null_Not_Provided()
    {
        var request = new UpdatePaymentTermRequest { Id = Guid.NewGuid(), Name = "Test" }; // DaysUntilDue is null
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.DaysUntilDue);
    }
}
