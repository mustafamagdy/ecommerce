using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Payments;

public class GetPaymentMethodsRequest:IRequest<List<PaymentMethodDto>>
{

}
public class GetMethodsHandler : IRequestHandler<GetPaymentMethodsRequest, List<PaymentMethodDto>>
{
    private readonly IReadRepository<PaymentMethod> _repository;
    private readonly IStringLocalizer<GetMethodsHandler> _t;

    public GetMethodsHandler(IReadRepository<PaymentMethod> repository, IStringLocalizer<GetMethodsHandler> localizer)
    {
        _repository = repository;
        _t = localizer;
    }

    public async Task<List<PaymentMethodDto>> Handle(GetPaymentMethodsRequest request, CancellationToken cancellationToken)
    {
        var methods = await _repository.ListAsync( cancellationToken);
        return methods.Select(a => new PaymentMethodDto(a.Id, a.Name)).ToList();
    }
}

public class PaymentMethodDto
{
    public Guid Id { get; }
    public string Name { get; }

    public PaymentMethodDto(Guid Id, string name)
    {
        this.Id = Id;
        Name = name;
    }
}