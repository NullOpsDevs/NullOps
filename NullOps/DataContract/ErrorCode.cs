namespace NullOps.DataContract;

public enum ErrorCode
{
    // Authentication/users
    Unauthorized,
    InvalidCredentials,
    UserAlreadyExists,
    RegistrationIsDisabled,
    
    // Request validation
    RequestValidationFailed,
    
    TestModeIsNotEnabled,
    
    InternalServerError
}
