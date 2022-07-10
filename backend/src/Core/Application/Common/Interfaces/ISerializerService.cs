
using Newtonsoft.Json;

namespace FSH.WebApi.Application.Common.Interfaces;

public interface ISerializerService : ITransientService
{
    string Serialize<T>(T obj);

    string Serialize<T>(T obj, Type type);

    T Deserialize<T>(string text);
    T Deserialize<T>(string text, JsonConverter converter);
}