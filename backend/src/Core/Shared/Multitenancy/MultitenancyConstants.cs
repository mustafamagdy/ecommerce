using Finbuckle.MultiTenant;

namespace FSH.WebApi.Shared.Multitenancy;

public sealed class MultitenancyConstants
{
  public static decimal MaxOneTimePaymentAmountForSubscription = 100000;
  public const string JobRunnerUserName = "JobRunner";
  public const string CashRegisterHeaderName = "cash-register";
  public const string SubscriptionTypeHeaderName = "subscription-type";
  public const string DefaultBranchName = "main-branch";

  public static class Root
  {
    public const string Id = "root";
    public const string Name = "Root";
    public const string EmailAddress = "admin@root.com";
  }

  public static RootTenantInfo RootTenant { get; } = new();

  public class RootTenantInfo : ITenantInfo
  {
    public string? Id { get => Root.Id; set => throw new InvalidOperationException("You cannot set root tenant Id"); }

    public string? Identifier { get => Root.Id; set => throw new InvalidOperationException("You cannot set root tenant Id"); }
    public string? Name { get => Root.Name; set => throw new InvalidOperationException("You cannot set root tenant Name"); }
    public string? ConnectionString { get => string.Empty; set => throw new NotImplementedException(); }
  }

  public const string DefaultPassword = "123Pa$$word!";
  public const string TempPassword = "123Pa$$word!";

  public const string TenantIdName = "tenant";
}