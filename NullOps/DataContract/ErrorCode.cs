namespace NullOps.DataContract;

public enum ErrorCode
{
    // Authentication
    Unauthorized,
    InvalidCredentials,
    
    // Request validation
    RequestValidationFailed,
    
    TestModeIsNotEnabled,
    
    InternalServerError
}
