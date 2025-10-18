using System.Net;
using NullOps.DataContract;

namespace NullOps.Exceptions;

public class DomainException(
    ErrorCode errorCode,
    string errorMessage,
    HttpStatusCode statusCode = HttpStatusCode.BadRequest,
    Dictionary<string, object>? details = null) : Exception
{
    public HttpStatusCode StatusCode { get; set; } = statusCode;
    public ErrorCode ErrorCode { get; set; } = errorCode;
    public string ErrorMessage { get; set; } = errorMessage;
    public Dictionary<string, object>? Details { get; set; } = details;
}
