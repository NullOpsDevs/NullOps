using System.ComponentModel.DataAnnotations;

namespace NullOps.DataContract.Request.Auth;

public class LoginRequest : BaseRequest
{
    [Required]
    [Length(4, 64)]
    public string Username { get; set; } = null!;

    [Required]
    [Length(4, 128)]
    public string Password { get; set; } = null!;
}