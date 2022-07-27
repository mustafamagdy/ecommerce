using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Multitenancy;

public abstract class TenantSubscriptionDto
{
  public TenantSubscriptionDto()
  {
    History = new();
  }

  public Guid SubscriptionId { get; set; } = default!;
  public string TenantId { get; set; } = default!;
  public DateTime ExpiryDate { get; set; }
  public List<SubscriptionHistoryDto> History { get; set; }
}

public class ProdTenantSubscriptionDto : TenantSubscriptionDto
{
  public ProdTenantSubscriptionDto()
  {
    Payments = new();
  }

  public List<SubscriptionPaymentDto> Payments { get; set; }
}

public class DemoTenantSubscriptionDto : TenantSubscriptionDto
{
}

public class TrainTenantSubscriptionDto : TenantSubscriptionDto
{
}

public class SubscriptionPaymentDto
{
  public Guid Id { get; set; }
  public decimal Amount { get; set; }
  public Guid PaymentMethodId { get; set; }
  public string PaymentMethodName { get; set; }
}

public class SubscriptionHistoryDto
{
  public DateTime StartDate { get; set; }
  public DateTime ExpiryDate { get; set; }
}