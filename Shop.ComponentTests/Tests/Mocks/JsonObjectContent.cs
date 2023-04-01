using MockServerClientNet.Model.Body;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Shop.ComponentTests.Tests.Mocks;

internal class JsonObjectContent<T> : BodyContent
{
    public JsonObjectContent(T value, JsonSerializerSettings? options = null)
    {
        options ??= new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };
        Type = "JSON";
        JsonValue = JsonConvert.SerializeObject(value, options);
        ContentTypeValue = "application/json";
    }
}