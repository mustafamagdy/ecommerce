using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Domain.Printing.Sections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FSH.WebApi.Domain.Serialization;

public class DocumentSectionJsonConverter : JsonConverter
{
  public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
  {
    var token = JToken.Load(reader);
    var typeToken = token["Type"].Value<string>();
    if (typeToken == null)
      throw new InvalidOperationException("invalid object");

    var typeVal = SectionType.FromName(typeToken);
    var actualType = typeVal.Name switch
    {
      nameof(SectionType.Logo) => typeof(LogoSection),
      nameof(SectionType.Barcode) => typeof(BarcodeSection),
      nameof(SectionType.Title) => typeof(TitleSection),
      nameof(SectionType.TwoPartTitle) => typeof(TwoItemRowSection),
      nameof(SectionType.Table) => typeof(TableSection),
      _ => throw new ArgumentOutOfRangeException(typeToken)
    };
    if (existingValue == null || existingValue.GetType() != actualType)
    {
      var contract = serializer.ContractResolver.ResolveContract(actualType);
      if (contract.DefaultCreatorNonPublic && contract.DefaultCreator == null)
      {
        throw new NotImplementedException($"No parameterless constructor found for {actualType}");
      }

      existingValue = contract.DefaultCreator();
    }

    using var subReader = token.CreateReader();
    serializer.Populate(subReader, existingValue);

    return existingValue;
  }

  public override bool CanConvert(Type objectType)
  {
    return typeof(DocumentSection).IsAssignableFrom(objectType);
  }

  public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
  {
    throw new NotImplementedException();
  }
}