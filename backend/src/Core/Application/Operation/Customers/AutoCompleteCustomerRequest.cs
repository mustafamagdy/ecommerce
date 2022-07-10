using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Customers;

public class BasicCustomerDto : IDto
{
  public Guid Id { get; set; }
  public string PhoneNumber { get; set; } = default!;
  public string Name { get; set; } = default!;
}

public class AutoCompleteCustomerSpec : Specification<Customer>, ISingleResultSpecification
{
  public AutoCompleteCustomerSpec(AutoCompleteCustomerRequest request) =>
    Query
      .Where(a => string.IsNullOrWhiteSpace(request.PhoneNumber) || a.PhoneNumber.Contains(request.PhoneNumber))
      .Where(a => string.IsNullOrWhiteSpace(request.Name) || a.Name.Contains(request.Name));
}

public class AutoCompleteCustomerRequest : IRequest<BasicCustomerDto>
{
  public string PhoneNumber { get; set; }
  public string Name { get; set; }
}

public class AutoCompleteCustomerRequestValidator : CustomValidator<AutoCompleteCustomerRequest>
{
  public AutoCompleteCustomerRequestValidator(IReadRepository<Customer> customerRepo, IStringLocalizer<IBaseRequest> t)
  {
    RuleFor(a => a.Name).NotEmpty().When(a => string.IsNullOrWhiteSpace(a.PhoneNumber));
    RuleFor(a => a.PhoneNumber).NotEmpty().When(a => string.IsNullOrWhiteSpace(a.Name));
  }
}

public class AutoCompleteCustomerRequestHandler : IRequestHandler<AutoCompleteCustomerRequest, BasicCustomerDto>
{
  private readonly IReadRepository<Customer> _repository;

  public AutoCompleteCustomerRequestHandler(IReadRepository<Customer> repository) => _repository = repository;

  public async Task<BasicCustomerDto> Handle(AutoCompleteCustomerRequest request, CancellationToken cancellationToken)
  {
    var spec = new AutoCompleteCustomerSpec(request);
    return await _repository.GetBySpecAsync((ISpecification<Customer, BasicCustomerDto>)spec, cancellationToken);
  }
}