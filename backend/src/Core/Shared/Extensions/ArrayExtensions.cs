namespace FSH.WebApi.Shared.Extensions;

public static class ArrayExtensions
{
  public static T[] And<T>(this T[] ar, T[] others)
  {
    return ar.Concat(others).ToArray();
  }
}