using FSH.WebApi.Application.Common.Mailing;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Multitenancy.EventHandlers;

public class SubscriptionRenewedEvent : DomainEvent
{
  public FSHTenantInfo Tenant { get; }
  public TenantProdSubscription Subscription { get; }
  public decimal Amount { get; }

  public SubscriptionRenewedEvent(FSHTenantInfo tenant, TenantProdSubscription subscription, decimal amount)
  {
    Tenant = tenant;
    Subscription = subscription;
    Amount = amount;
  }
}

public class SubscriptionRenewedEventHandler : EventNotificationHandler<SubscriptionRenewedEvent>
{
  private readonly ILogger<SubscriptionRenewedEventHandler> _logger;
  private readonly IStringLocalizer _t;
  private readonly IEmailTemplateService _templateService;
  private readonly IJobService _jobService;
  private readonly IMailService _mailService;

  public SubscriptionRenewedEventHandler(ILogger<SubscriptionRenewedEventHandler> logger, IStringLocalizer<SubscriptionRenewedEventHandler> localizer,
    IEmailTemplateService templateService, IJobService jobService, IMailService mailService)
  {
    _logger = logger;
    _t = localizer;
    _templateService = templateService;
    _jobService = jobService;
    _mailService = mailService;
  }

  public override Task Handle(SubscriptionRenewedEvent @event, CancellationToken cancellationToken)
  {
    var eMailModel = new SubscriptionRenewedModel
    {
      Amount = @event.Amount,
      TenantName = @event.Tenant.Name,
      SubscriptionExpiryDate = @event.Subscription.ExpiryDate
    };

    var mailRequest = new MailRequest(
      new List<string> { @event.Tenant.AdminEmail },
      _t["Subscription Created"],
      _templateService.GenerateEmailTemplate("subscription-renewed", eMailModel));

    _jobService.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));

    return Task.CompletedTask;
  }
}