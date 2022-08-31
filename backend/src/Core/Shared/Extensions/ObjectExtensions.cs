using System.Reflection;

namespace FSH.WebApi.Shared.Extensions;

public static class StringExtensions
{
  public static string IfNullOrEmpty(this string? str, string defaultVal) => string.IsNullOrEmpty(str) ? defaultVal : str;
}

public static class ObjectExtensions
{
  public static T? TryGetPropertyValue<T>(this object? obj, string propertyName, T? defaultValue = default) =>
    obj?.GetType().GetRuntimeProperty(propertyName) is { } propertyInfo
      ? (T?)propertyInfo.GetValue(obj)
      : defaultValue;

  public static string[] EvaluatePropertyValues<T>(this string exp, T obj)
    where T : notnull
  {
    var exps = exp.Split(',');
    return exps.Select(e => EvaluatePropertyValue(e, obj)).ToArray();
  }

  public static string EvaluatePropertyValue<T>(this string exp, T obj)
    where T : notnull
  {
    if (!exp.StartsWith("$"))
    {
      return exp;
    }

    var propExp = exp[1..];

    // $name -> done
    // $sub.name -> done
    // $sub.sub.name -> done
    // $items[].name
    var propValue = GetPropertyValue(propExp, obj);
    var value = (propValue ?? "").ToString();
    return value;
  }

  public static object? GetPropertyValue(this string exp, object obj)
  {
    if (!exp.Contains('.'))
    {
      return obj.TryGetPropertyValue<object?>(exp);
    }

    string[] propNames = exp.Split('.');
    foreach (var propName in propNames)
    {
      PropertyInfo prop = null;
      if (propName.EndsWith("[]"))
      {
        var arrayName = propName[..^2];
      }
      else
      {
        prop = obj.GetType().GetProperty(propName);
      }

      obj = prop.GetValue(obj, null);
    }

    return obj;
  }
}