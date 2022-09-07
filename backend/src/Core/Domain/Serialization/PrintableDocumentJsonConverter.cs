using FSH.WebApi.Domain.Printing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FSH.WebApi.Domain.Serialization;

public class PrintableDocumentJsonConverter : JsonConverter
{
  public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
  {
    JToken token = JToken.Load(reader);
    var typeToken = token["Type"].Value<string>();
    if (typeToken == null)
      throw new InvalidOperationException("invalid object");

    var typeVal = PrintableType.FromName(typeToken);
    var actualType = typeVal.Name switch
    {
      nameof(PrintableType.Receipt) => typeof(SimpleReceiptInvoice),
      nameof(PrintableType.Wide) => typeof(WideReceiptInvoice),
      nameof(PrintableType.OrdersSummary) => typeof(OrdersSummaryReport),
      _ => throw new ArgumentOutOfRangeException(typeToken)
    };

    // var jsonSections = item["Sections"].ToString();
    // var sections = JsonConvert.DeserializeObject<List<DocumentSection>>(jsonSections, new DocumentSectionJsonConverter());
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
    return typeof(PrintableDocument).IsAssignableFrom(objectType);
  }

  public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
  {
    throw new NotImplementedException();
  }
}