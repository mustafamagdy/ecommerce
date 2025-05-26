using Ardalis.SmartEnum;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;

namespace FSH.WebApi.Infrastructure.OpenApi.PrimitiveTypeMappings;

public class SmartEnumTypeMapper<TEnum, TVal> : PrimitiveTypeMapper
  where TEnum : SmartEnum<TEnum, TVal>
  where TVal : IComparable<TVal>, IEquatable<TVal>
{
  internal SmartEnumTypeMapper()
    : base(typeof(TEnum), x =>
    {
      x.Type = JsonObjectType.String;
      foreach (var item in SmartEnum<TEnum, TVal>.List)
      {
        x.Enumeration.Add(item.Name);
      }
    })
  {
  }
}

public class SmartEnumTypeMapper<T> : SmartEnumTypeMapper<T, int>
  where T : SmartEnum<T>
{
}