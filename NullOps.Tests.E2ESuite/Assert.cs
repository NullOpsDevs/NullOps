using System.Net;
using System.Text.Json;
using NullOps.Serializers;
using NullOps.Tests.E2ESuite.Exceptions;
using NullOps.Tests.E2ESuite.Extensions;
using Refit;

namespace NullOps.Tests.E2ESuite;

public static class Assert
{
    public static void ExpectTrue(bool? condition, string message)
    {
        if(condition == true)
            return;
        
        throw new TestException(message);
    }
    
    public static void ExpectFalse(bool? condition, string message)
    {
        if(condition == false)
            return;
        
        throw new TestException(message);
    }

    private static string SerializeContent<T>(IApiResponse<T> apiResponse) where T : class
    {
        return JsonSerializer.Serialize(apiResponse.GetContent(), GlobalJsonSerializerOptions.Options);
    }

    public static void IsSuccessStatusCode<T>(IApiResponse<T> apiResponse) where T : class
    {
        if(apiResponse.IsSuccessStatusCode)
            return;
        
        throw new TestException($"API call failed with status code {apiResponse.StatusCode} {apiResponse.ReasonPhrase}\n{SerializeContent(apiResponse)}");
    }

    public static void IsUnsuccessfulStatusCode<T>(IApiResponse<T> apiResponse) where T : class
    {
        if(!apiResponse.IsSuccessStatusCode)
            return;
        
        throw new TestException($"API call succeeded with status code {apiResponse.StatusCode} {apiResponse.ReasonPhrase}\n{SerializeContent(apiResponse)}");
    }

    public static void ExpectStatusCode<T>(IApiResponse<T> apiResponse, HttpStatusCode expectedStatusCode) where T : class
    {
        if(apiResponse.StatusCode == expectedStatusCode)
            return;
        
        throw new TestException($"API call returned status code '{apiResponse.StatusCode:D} {apiResponse.StatusCode:G}', expected '{expectedStatusCode:D} {expectedStatusCode:G}'\n{SerializeContent(apiResponse)}");
    }

    public static void IsNull<T>(T? value, string message)
    {
        if(value == null)
            return;
        
        throw new TestException(message);
    }
    
    public static void IsNotNull<T>(T? value, string message)
    {
        if(value != null)
            return;
        
        throw new TestException(message);
    }
    
    public static void IsMeaningfulString(string? value, string message)
    {
        if(!string.IsNullOrEmpty(value))
            return;
        
        throw new TestException(message);
    }

    public static void Must<T>(T? value, Func<T, bool> predicate, string message)
    {
        if(value == null)
            throw new TestException("Unable to check 'Must' assert - value is NULL!");
        
        if(predicate(value))
            return;
        
        throw new TestException(message);
    }
    
    public static void Fail(string message)
    {
        throw new TestException(message);
    }
}
