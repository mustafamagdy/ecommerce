using System.Reflection;

namespace FSH.WebApi.Shared.Extensions;

internal static class ObjectExtensions
{
  public static T? TryGetPropertyValue<T>(this object? obj, string propertyName, T? defaultValue = default) =>
    obj?.GetType().GetRuntimeProperty(propertyName) is { } propertyInfo
      ? (T?)propertyInfo.GetValue(obj)
      : defaultValue;
}