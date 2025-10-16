using System.Net;
using NullOps.DataContract;
using NullOps.Exceptions;
using NullOps.Serializers;

namespace NullOps.Middlewares;

public class ExceptionHandlerMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (DomainException domainException)
        {
            context.Response.StatusCode = (int) domainException.StatusCode;
            await context.Response.WriteAsJsonAsync(new BaseResponse
            {
                Success = false,
                Error = new ResponseError
                {
                    Code = domainException.ErrorCode,
                    Message = domainException.ErrorMessage,
                    Details = domainException.Details
                }
            });
        }
        catch (Exception exception)
        {
            context.RequestServices.GetRequiredService<ILogger<ExceptionHandlerMiddleware>>()
                .LogError(exception, "Exception occured while processing request '{RequestPath}'.", context.Request.Path);
            
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(BaseResponse.InternalServerError, GlobalJsonSerializerOptions.Options);
        }
    }
}