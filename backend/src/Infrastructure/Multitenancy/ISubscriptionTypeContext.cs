using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Shared.Exceptions;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public interface ISubscriptionTypeContext : IScopedService
{
  public Func<SubscriptionType> SubscriptionType { get; set; }
}

public class SubscriptionTypeContext : ISubscriptionTypeContext
{
  public Func<SubscriptionType> SubscriptionType { get; set; }
}