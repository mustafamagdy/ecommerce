namespace FSH.WebApi.Application.Operation.Customers;

public class CustomerDto : IDto
{
  public string PhoneNumber { get; set; } = default!;
  public string Name { get; set; } = default!;
}