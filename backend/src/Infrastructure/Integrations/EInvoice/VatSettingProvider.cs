using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Settings.Vat;

namespace FSH.WebApi.Infrastructure.Integrations.EInvoice;

public sealed class VatSettingProvider : IVatSettingProvider
{
  private readonly ITenantInfo _currentTenant;

  public VatSettingProvider(ITenantInfo currentTenant)
  {
    _currentTenant = currentTenant;
  }

  // todo: can multiple tenant share same legal company profile?
  // store that in db
  public string LegalEntityName => _currentTenant.Name;
  public string VatRegNo => _currentTenant.Name + " VAT0123456";
}