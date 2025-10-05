using System.Net;
using NullOps.Tests.E2ESuite.Exceptions;
using Refit;

namespace NullOps.Tests.E2ESuite;

public static class Assert
{
    public static void IsTrue(bool? condition, string message)
    {
        if(condition == true)
            return;
        
        throw new TestException(message);
    }
    
    public static void IsFalse(bool? condition, string message)
    {
        if(condition == false)
            return;
        
        throw new TestException(message);
    }

    public static void IsSuccessStatusCode(IApiResponse apiResponse)
    {
        if(apiResponse.IsSuccessStatusCode)
            return;
        
        throw new TestException($"API call failed with status code {apiResponse.StatusCode} {apiResponse.ReasonPhrase}");
    }

    public static void IsUnsuccessfulStatusCode(IApiResponse apiResponse)
    {
        if(!apiResponse.IsSuccessStatusCode)
            return;
        
        throw new TestException($"API call succeeded with status code {apiResponse.StatusCode} {apiResponse.ReasonPhrase}");
    }

    public static void ExpectStatusCode(IApiResponse apiResponse, HttpStatusCode expectedStatusCode)
    {
        if(apiResponse.StatusCode == expectedStatusCode)
            return;
        
        throw new TestException($"API call returned status code {apiResponse.StatusCode} {apiResponse.ReasonPhrase}, expected {expectedStatusCode:D} {expectedStatusCode:G}");
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
    
    public static void Fail(string message)
    {
        throw new TestException(message);
    }
}
