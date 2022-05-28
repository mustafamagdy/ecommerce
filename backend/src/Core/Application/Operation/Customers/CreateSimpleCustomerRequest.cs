namespace FSH.WebApi.Application.Operation.Customers;

public class CreateSimpleCustomerRequest : IRequest<CustomerDto>
{
  public string PhoneNumber { get; set; }
  public string Name { get; set; }
}