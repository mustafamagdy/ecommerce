namespace FSH.WebApi.Shared.Extensions;

public static class StringExtensions
{
  public static string IfNullOrEmpty(this string? str, string defaultVal) => string.IsNullOrEmpty(str) ? defaultVal : str;
}