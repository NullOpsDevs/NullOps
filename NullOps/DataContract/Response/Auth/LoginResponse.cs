using System.ComponentModel.DataAnnotations;

namespace NullOps.DataContract.Response.Auth;

public class LoginResponse
{
    [Required]
    public required string Token { get; set; }
}
