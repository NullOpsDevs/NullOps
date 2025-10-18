using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NullOps.DataContract;

public class ResponseError
{
    [Required]
    public required ErrorCode Code { get; set; }
    
    [Required]
    public required string Message { get; set; }
    
    public Dictionary<string, object>? Details { get; set; }
}

public class BaseResponse
{
    public static readonly BaseResponse Successful = new() { Success = true };
    public static readonly BaseResponse InternalServerError = new() { Success = false, Error = new ResponseError { Code = ErrorCode.InternalServerError, Message = "Internal server error has occured" } };
    public static readonly BaseResponse Unauthorized = new() { Success = false, Error = new ResponseError { Code = ErrorCode.Unauthorized, Message = "This resource is protected and requires authentication" } };
    
    [Required]
    public required bool Success { get; set; }
    
    public ResponseError? Error { get; set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BaseResponse CreateSuccessful() => new() { Success = true };
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BaseResponse CreateFailed() => new() { Success = false };

    public BaseResponse WithError(ErrorCode errorCode, string? message = null, Dictionary<string, object>? details = null)
    {
        Error = new ResponseError
        {
            Code = errorCode,
            Message = message ?? string.Empty,
            Details = details
        };

        return this;
    }
}

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public class BaseResponse<T>
{
    public static readonly BaseResponse Successful = new() { Success = true };
    public static readonly BaseResponse InternalServerError = new() { Success = false, Error = new ResponseError { Code = ErrorCode.InternalServerError, Message = "Internal server error has occured" } };
    public static readonly BaseResponse Unauthorized = new() { Success = false, Error = new ResponseError { Code = ErrorCode.Unauthorized, Message = "This resource is protected and requires authentication" } };
    
    [Required]
    public required bool Success { get; set; }
    
    public ResponseError? Error { get; set; }
    
    public T? Data { get; set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BaseResponse<T> CreateSuccessful() => new() { Success = true };
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BaseResponse<T> CreateFailed() => new() { Success = false };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BaseResponse<T> WithError(ErrorCode errorCode, string? message = null, Dictionary<string, object>? details = null)
    {
        Error = new ResponseError
        {
            Code = errorCode,
            Message = message ?? string.Empty,
            Details = details
        };

        return this;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BaseResponse<T> WithData(T data)
    {
        Data = data;
        return this;
    }
}
