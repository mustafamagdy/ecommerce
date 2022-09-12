namespace FSH.WebApi.Infrastructure.Multitenancy;

public sealed class FeatureNotAllowedException : Exception
{
}

public sealed class SubscriptionExpiredException : Exception
{
  public SubscriptionExpiredException(DateTime expiryDate, string message)
    : base(message)
  {
    ExpiryDate = expiryDate;
  }

  public DateTime ExpiryDate { get; set; }
}