namespace FSH.WebApi.Application.Common.Exporters;

public interface IPdfWriter : ITransientService
{
  Stream WriteToStream(dynamic data);
}