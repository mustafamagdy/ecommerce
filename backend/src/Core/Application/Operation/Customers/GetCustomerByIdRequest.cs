using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Customers;

public record GetCustomerByIdRequest(Guid Id) : IRequest<BasicCustomerDto>;
public class GetCustomerByIdRequestSpec : Specification<Customer, BasicCustomerDto>,ISingleResultSpecification
{
    public GetCustomerByIdRequestSpec(Guid id) =>
        Query
            .Where(a => a.Id == id);
}

public class GetCustomerByIdRequestHandler : IRequestHandler<GetCustomerByIdRequest, BasicCustomerDto>
{
    private readonly IReadRepository<Customer> _repository;

    public GetCustomerByIdRequestHandler(IReadRepository<Customer> repository) => _repository = repository;

    public async Task<BasicCustomerDto> Handle(GetCustomerByIdRequest request, CancellationToken cancellationToken)
    {
        var spec = new GetCustomerByIdRequestSpec(request.Id);
        return await _repository.GetBySpecAsync(spec,  cancellationToken);
    }
}
