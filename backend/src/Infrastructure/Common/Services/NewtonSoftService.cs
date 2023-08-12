using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Infrastructure.Seeders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace FSH.WebApi.Infrastructure.Common.Services;

public sealed class NewtonsoftService : ISerializerService
{
  public T Deserialize<T>(string text)
    => JsonConvert.DeserializeObject<T>(text);

  public T Deserialize<T>(string text, JsonConverter converter)
    => JsonConvert.DeserializeObject<T>(text, converter);

  public string Serialize<T>(T obj)
  {
    return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
    {
      ContractResolver = new CamelCasePropertyNamesContractResolver(),
      NullValueHandling = NullValueHandling.Ignore,
      Converters = new List<JsonConverter>
      {
        new StringEnumConverter() { CamelCaseText = true },
      }
    });
  }

  public string Serialize<T>(T obj, Type type)
  {
    return JsonConvert.SerializeObject(obj, type, new());
  }
}