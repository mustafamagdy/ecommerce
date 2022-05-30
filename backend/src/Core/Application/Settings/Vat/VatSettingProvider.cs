namespace FSH.WebApi.Application.Settings.Vat;

public interface IVatSettingProvider : ITransientService
{
  public string LegalEntityName { get; }
  public string VatRegNo { get; }
}