namespace NullOps.DataContract;

public enum ErrorCode
{
    // Authentication/users
    Unauthorized,
    InvalidCredentials,
    UserAlreadyExists,
    RegistrationIsDisabled,
    InsufficientPermissions,
    
    // Request validation
    RequestValidationFailed,
    
    // General errors
    NotFound,
    
    // Test mode
    TestModeIsNotEnabled,
    
    InternalServerError
}
