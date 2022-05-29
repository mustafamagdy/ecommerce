namespace FSH.WebApi.Application.Common.Exporters;

public interface IPdfGenerator : ITransientService
{
  Stream WriteToStream<T>(dynamic data);
}