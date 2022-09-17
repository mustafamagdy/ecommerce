using FSH.WebApi.Application.Common.Mailing;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Multitenancy.EventHandlers;

public class TenantPayForSubscriptionEvent : DomainEvent
{
  public FSHTenantInfo Tenant { get; }
  public TenantProdSubscription Subscription { get; }
  public PaymentMethod PaymentMethod { get; }
  public decimal Amount { get; }

  public TenantPayForSubscriptionEvent(FSHTenantInfo tenant, TenantProdSubscription subscription, PaymentMethod pm, decimal amount)
  {
    Tenant = tenant;
    Subscription = subscription;
    PaymentMethod = pm;
    Amount = amount;
  }
}

public class TenantPayForSubscriptionEventHandler : EventNotificationHandler<TenantPayForSubscriptionEvent>
{
  private readonly ILogger<TenantPayForSubscriptionEventHandler> _logger;
  private readonly IStringLocalizer _t;
  private readonly IEmailTemplateService _templateService;
  private readonly IJobService _jobService;
  private readonly IMailService _mailService;

  public TenantPayForSubscriptionEventHandler(ILogger<TenantPayForSubscriptionEventHandler> logger, IStringLocalizer<TenantPayForSubscriptionEventHandler> localizer,
    IEmailTemplateService templateService, IJobService jobService, IMailService mailService)
  {
    _logger = logger;
    _t = localizer;
    _templateService = templateService;
    _jobService = jobService;
    _mailService = mailService;
  }

  public override Task Handle(TenantPayForSubscriptionEvent @event, CancellationToken cancellationToken)
  {
    var eMailModel = new PayForSubscriptionEmailModel()
    {
      Amount = @event.Amount,
      TenantName = @event.Tenant.Name,
      SubscriptionExpiryDate = @event.Subscription.ExpiryDate,
      PaymentMethodName = @event.PaymentMethod.Name
    };

    var mailRequest = new MailRequest(
      new List<string> { @event.Tenant.AdminEmail },
      _t["Subscription Created"],
      _templateService.GenerateEmailTemplate("pay-for-subscription", eMailModel));

    _jobService.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));

    return Task.CompletedTask;
  }
}