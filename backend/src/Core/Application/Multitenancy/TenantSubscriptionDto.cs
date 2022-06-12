namespace FSH.WebApi.Application.Multitenancy;

public class TenantSubscriptionDto
{
  public string Id { get; set; } = default!;
  public string TenantId { get; set; } = default!;
  public DateTime ExpiryDate { get; set; }
  public bool IsDemo { get; set; }

  public List<SubscriptionPaymentDto> Payments { get; set; }
}

public class SubscriptionPaymentDto
{
  public decimal Amount { get; set; }
  public Guid PaymentMethodId { get; set; }
  public string PaymentMethodName { get; set; }
}