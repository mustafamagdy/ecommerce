namespace FSH.WebApi.Application.Multitenancy;

public class TenantSubscriptionDto
{
  public TenantSubscriptionDto()
  {
    Payments = new();
    History = new();
  }

  public Guid Id { get; set; } = default!;
  public string TenantId { get; set; } = default!;
  public DateTime ExpiryDate { get; set; }
  public List<SubscriptionPaymentDto> Payments { get; set; }
  public List<SubscriptionHistoryDto> History { get; set; }
}

public class SubscriptionPaymentDto
{
  public decimal Amount { get; set; }
  public Guid PaymentMethodId { get; set; }
  public string PaymentMethodName { get; set; }
}

public class SubscriptionHistoryDto
{
  public DateTime StartDate { get; set; }
  public DateTime ExpireDate { get; set; }
}