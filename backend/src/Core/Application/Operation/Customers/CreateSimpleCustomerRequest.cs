using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Persistence;
using Mapster;

namespace FSH.WebApi.Application.Operation.Customers;

public class CreateSimpleCustomerRequest : IRequest<BasicCustomerDto>
{
  public string PhoneNumber { get; set; } = default!;
  public string Name { get; set; } = default!;
}

public class GetCustomerByPhoneNumberSpec : Specification<Customer>, ISingleResultSpecification
{
  public GetCustomerByPhoneNumberSpec(string phoneNumber) => Query.Where(a => a.PhoneNumber == phoneNumber);
}

public class CreateSimpleCustomerRequestValidator : CustomValidator<CreateSimpleCustomerRequest>
{
  public CreateSimpleCustomerRequestValidator(IReadRepository<Customer> customerRepo, IStringLocalizer<IBaseRequest> t)
  {
    RuleFor(a => a.Name)
      .NotEmpty()
      .MaximumLength(1024);

    RuleFor(a => a.PhoneNumber)
      .MustAsync(async (phoneNumber, ct) => await customerRepo.FirstOrDefaultAsync(new GetCustomerByPhoneNumberSpec(phoneNumber), ct) is null)
      .WithMessage((_, phoneNumber) => t["Customer with the same phone number already Exists."]);
  }
}

public class CreateSimpleCustomerRequestHandler : IRequestHandler<CreateSimpleCustomerRequest, BasicCustomerDto>
{
  private readonly IRepositoryWithEvents<Customer> _customerRepo;
  private readonly IStringLocalizer _t;
  private readonly IApplicationUnitOfWork _uow;

  public CreateSimpleCustomerRequestHandler(IRepositoryWithEvents<Customer> customerRepo, IStringLocalizer<CreateSimpleCustomerRequestHandler> localizer, IApplicationUnitOfWork uow)
  {
    _customerRepo = customerRepo;
    _t = localizer;
    _uow = uow;
  }

  public async Task<BasicCustomerDto> Handle(CreateSimpleCustomerRequest request, CancellationToken cancellationToken)
  {
    var customer = new Customer(request.Name, request.PhoneNumber);
    customer = await _customerRepo.AddAsync(customer, cancellationToken);
    if (customer is null)
    {
      throw new InternalServerException(_t["Failed to save new customer data"]);
    }

    await _uow.CommitAsync(cancellationToken);
    return customer.Adapt<BasicCustomerDto>();
  }
}