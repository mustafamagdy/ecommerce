namespace FSH.WebApi.Infrastructure.Multitenancy;

public class FeatureNotAllowedException : Exception
{
}

public class SubscriptionExpiredException : Exception
{
  public SubscriptionExpiredException(DateTime expiryDate, string message)
    : base(message)
  {
    ExpiryDate = expiryDate;
  }

  public DateTime ExpiryDate { get; set; }
}