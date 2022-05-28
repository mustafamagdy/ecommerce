using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Customers;

public class GetCustomerByPhoneNumberSpec : Specification<Customer>, ISingleResultSpecification
{
  public GetCustomerByPhoneNumberSpec(string phoneNumber) => Query.Where(a => a.PhoneNumber == phoneNumber);
}