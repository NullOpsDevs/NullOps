using System.Text.Json;
using System.Text.Json.Serialization;

namespace NullOps.Serializers;

public static class GlobalJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}