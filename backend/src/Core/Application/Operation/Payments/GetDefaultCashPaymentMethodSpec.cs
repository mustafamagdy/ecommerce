using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Payments;

public class GetDefaultCashPaymentMethodSpec : Specification<PaymentMethod>, ISingleResultSpecification
{
  public GetDefaultCashPaymentMethodSpec() => Query.Where(a => a.CashDefault);
}