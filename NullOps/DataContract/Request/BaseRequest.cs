using System.ComponentModel.DataAnnotations;
using System.Net;
using NullOps.Exceptions;

namespace NullOps.DataContract.Request;

public abstract class BaseRequest
{
    public void ValidateOrThrow()
    {
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();

        if (Validator.TryValidateObject(this, validationContext, validationResults, true))
            return;

        var errors = validationResults
            .Where(x => x.ErrorMessage != null)
            .Select(x => x.ErrorMessage!)
            .Distinct();

        throw new DomainException(ErrorCode.RequestValidationFailed, "Request validation failed", HttpStatusCode.UnprocessableEntity, new()
        {
            {"validation_errors", errors}
        });
    }
}
