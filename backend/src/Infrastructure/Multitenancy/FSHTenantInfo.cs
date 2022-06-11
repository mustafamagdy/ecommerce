using System.ComponentModel.DataAnnotations.Schema;
using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
using MassTransit;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public class TenantSubscriptionInfo
{
  public string Id { get; set; }
  public string TenantId { get; set; }
  public DateTime ExpiryDate { get; private set; }
  public bool IsDemo { get; private set; }

  public TenantSubscriptionInfo Renew(DateTime newExpiryDate)
  {
    ExpiryDate = newExpiryDate;
    return this;
  }

  public static TenantSubscriptionInfo CreateDemoForDays(string tenantId, int days)
  {
    return new TenantSubscriptionInfo
    {
      Id = NewId.Next().ToString(),
      TenantId = tenantId,
      ExpiryDate = DateTime.Now.AddDays(days),
      IsDemo = true
    };
  }

  public static TenantSubscriptionInfo CreateProdForDays(string tenantId, int days)
  {
    return new TenantSubscriptionInfo
    {
      Id = NewId.Next().ToString(),
      TenantId = tenantId,
      ExpiryDate = DateTime.Now.AddDays(days),
      IsDemo = false
    };
  }
}

