using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Customers;

public class SimpleCustomerData
{
  public string PhoneNumber { get; set; } = default!;
  public string Name { get; set; } = default!;
}

public class CreateSimpleCustomerRequestValidator : CustomValidator<SimpleCustomerData>
{
  public CreateSimpleCustomerRequestValidator(IReadRepository<Customer> customerRepo, IStringLocalizer<IBaseRequest> t)
  {
    RuleFor(a => a.Name)
      .NotEmpty()
      .MaximumLength(1024);

    RuleFor(a => a.PhoneNumber)
      .MustAsync(async (phoneNumber, ct) => await customerRepo.GetBySpecAsync(new GetCustomerByPhoneNumberSpec(phoneNumber), ct) is null)
      .WithMessage((_, phoneNumber) => t["Customer with the same phone number already Exists."]);
  }
}