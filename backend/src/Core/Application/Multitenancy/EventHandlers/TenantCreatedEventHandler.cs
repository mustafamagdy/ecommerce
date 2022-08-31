using FSH.WebApi.Application.Common.Mailing;
using FSH.WebApi.Domain.MultiTenancy;

namespace FSH.WebApi.Application.Multitenancy.EventHandlers;

public class TenantCreatedEvent : DomainEvent
{
  public FSHTenantInfo Tenant { get; }
  public List<TenantSubscription> Subscriptions { get; }

  public TenantCreatedEvent(FSHTenantInfo tenant, List<TenantSubscription> subscriptions)
  {
    Tenant = tenant;
    Subscriptions = subscriptions;
  }
}

public class TenantCreatedEventHandler : EventNotificationHandler<TenantCreatedEvent>
{
  private readonly ILogger<TenantCreatedEventHandler> _logger;
  private readonly IStringLocalizer _t;
  private readonly IEmailTemplateService _templateService;
  private readonly IJobService _jobService;
  private readonly IMailService _mailService;

  public TenantCreatedEventHandler(ILogger<TenantCreatedEventHandler> logger, IStringLocalizer<TenantCreatedEventHandler> localizer,
    IEmailTemplateService templateService, IJobService jobService, IMailService mailService)
  {
    _logger = logger;
    _t = localizer;
    _templateService = templateService;
    _jobService = jobService;
    _mailService = mailService;
  }

  public override Task Handle(TenantCreatedEvent @event, CancellationToken cancellationToken)
  {
    string prodUrl = $"https://prod.abcd.com/{@event.Tenant.Key}";

    var eMailModel = new TenantCreatedEmailModel()
    {
      AdminEmail = @event.Tenant.AdminEmail,
      TenantName = @event.Tenant.Name,
      // todo notify for all subscriptions
      SubscriptionExpiryDate = @event.Subscriptions.FirstOrDefault().ExpiryDate,
      SiteUrl = prodUrl
    };

    var mailRequest = new MailRequest(
      new List<string> { @event.Tenant.AdminEmail },
      _t["Subscription Created"],
      _templateService.GenerateEmailTemplate("email-subscription", eMailModel));

    _jobService.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));

    return Task.CompletedTask;
  }
}