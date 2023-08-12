namespace FSH.WebApi.Application.Common.Interfaces;

public interface ISystemTime : ITransientService
{
  DateTime Now { get; }
  DateTime UtcNow { get; }
}