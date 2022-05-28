namespace FSH.WebApi.Application.Operation.Customers;

public class CustomerDto : IDto
{
  public string PhoneNumber { get; set; }
  public string Name { get; set; }
}