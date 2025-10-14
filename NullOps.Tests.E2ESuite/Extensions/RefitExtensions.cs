using System.Text.Json;
using NullOps.Serializers;
using NullOps.Tests.E2ESuite.Exceptions;
using Refit;

namespace NullOps.Tests.E2ESuite.Extensions;

public static class RefitExtensions
{
    public static T? GetContent<T>(this IApiResponse<T> response)
        where T : class
    {
        if(response.IsSuccessStatusCode)
            return response.Content;

        if(response.Error.InnerException != null)
            throw new TestException(response.Error.InnerException.Message);
        
        if (response.Error.Content == null)
            return null;
        
        return JsonSerializer.Deserialize<T>(response.Error.Content, GlobalJsonSerializerOptions.Options);
    }

    public static object? GetContent(this IApiResponse response)
    {
        if (response.IsSuccessStatusCode)
            return null; // no typed content available here; returning null

        if (response.Error == null || response.Error.Content == null)
            return null;

        try
        {
            return JsonSerializer.Deserialize<object>(response.Error.Content, GlobalJsonSerializerOptions.Options);
        }
        catch
        {
            return response.Error.Content;
        }
    }
}
