using System.Text.Json;
using Refit;

namespace NullOps.Tests.E2ESuite.Extensions;

public static class RefitExtensions
{
    public static T? GetContent<T>(this IApiResponse<T> response)
        where T : class
    {
        if(response.IsSuccessStatusCode)
            return response.Content;

        if (response.Error.Content == null)
            return null;
        
        return JsonSerializer.Deserialize<T>(response.Error.Content, JsonSerializerOptions.Web);
    }
}