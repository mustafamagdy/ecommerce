namespace FSH.WebApi.Domain.Common.Contracts;

public interface IHaveImageAndIcon
{
  public string? ImagePath { get; }
  public string? IconPath { get; }
}