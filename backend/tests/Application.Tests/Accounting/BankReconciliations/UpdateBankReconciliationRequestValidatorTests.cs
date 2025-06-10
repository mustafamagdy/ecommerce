using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.BankReconciliations;
using FSH.WebApi.Domain.Accounting; // For ReconciliationStatus enum
using System;

namespace Application.Tests.Accounting.BankReconciliations;

public class UpdateBankReconciliationRequestValidatorTests
{
    private readonly UpdateBankReconciliationRequestValidator _validator = new();

    private UpdateBankReconciliationRequest CreateValidRequest(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        ManuallyAssignedUnclearedChecks = 100m,
        ManuallyAssignedDepositsInTransit = 50m,
        Status = ReconciliationStatus.InProgress
    };

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid_With_All_Fields()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_OptionalFields_Are_Null()
    {
        var request = new UpdateBankReconciliationRequest
        {
            Id = Guid.NewGuid(),
            ManuallyAssignedUnclearedChecks = null,
            ManuallyAssignedDepositsInTransit = null,
            Status = null
        };
        var result = _validator.TestValidate(request);
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
    public void Should_Have_Error_When_ManuallyAssignedUnclearedChecks_Is_Negative()
    {
        var request = CreateValidRequest();
        request.ManuallyAssignedUnclearedChecks = -50m;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ManuallyAssignedUnclearedChecks);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ManuallyAssignedUnclearedChecks_Is_Zero()
    {
        var request = CreateValidRequest();
        request.ManuallyAssignedUnclearedChecks = 0m;
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ManuallyAssignedUnclearedChecks);
    }

    [Fact]
    public void Should_Have_Error_When_ManuallyAssignedDepositsInTransit_Is_Negative()
    {
        var request = CreateValidRequest();
        request.ManuallyAssignedDepositsInTransit = -20m;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ManuallyAssignedDepositsInTransit);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ManuallyAssignedDepositsInTransit_Is_Zero()
    {
        var request = CreateValidRequest();
        request.ManuallyAssignedDepositsInTransit = 0m;
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ManuallyAssignedDepositsInTransit);
    }

    [Fact]
    public void Should_Have_Error_When_Status_Is_InvalidEnumValue_If_Provided()
    {
        var request = CreateValidRequest();
        request.Status = (ReconciliationStatus)99; // Invalid enum
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
