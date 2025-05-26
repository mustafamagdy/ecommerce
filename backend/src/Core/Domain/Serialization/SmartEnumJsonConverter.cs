using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.SmartEnum;

namespace FSH.WebApi.Domain.Serialization;

public class SmartEnumJsonConverter<TEnum, TValue> : JsonConverter<TEnum>
  where TEnum : SmartEnum<TEnum, TValue>
  where TValue : IComparable<TValue>, IEquatable<TValue>
{
  public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    if (reader.TokenType != JsonTokenType.String)
    {
      throw new JsonException();
    }

    string value = reader.GetString();

    // Use SmartEnum's FromValue or FromName depending on TValue
    TEnum enumValue;
    if (typeof(TValue) == typeof(string))
    {
      enumValue = SmartEnum<TEnum, TValue>.FromName(value, ignoreCase: true);
    }
    else
    {
      throw new JsonException("Unsupported TValue type for SmartEnumJsonConverter.");
    }

    if (enumValue == null)
    {
      throw new JsonException($"Value '{value}' is not valid for enum type {typeof(TEnum).Name}.");
    }

    return enumValue;
  }

  public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
  {
    if (typeof(TValue) == typeof(string))
    {
      writer.WriteStringValue(value.Name);
    }
    else
    {
      throw new JsonException("Unsupported TValue type for SmartEnumJsonConverter.");
    }
  }
}
