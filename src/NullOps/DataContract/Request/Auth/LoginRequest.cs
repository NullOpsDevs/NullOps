using System.ComponentModel.DataAnnotations;

namespace NullOps.DataContract.Request.Auth;

public class LoginRequest : BaseRequest
{
    [Required]
    [Length(Limits.Users.MinUsernameLength, Limits.Users.MaxUsernameLength)]
    public string Username { get; set; } = null!;

    [Required]
    [Length(Limits.Users.MinPasswordLength, Limits.Users.MaxPasswordLength)]
    public string Password { get; set; } = null!;
}