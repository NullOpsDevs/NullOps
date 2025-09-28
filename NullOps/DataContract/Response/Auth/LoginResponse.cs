using System.ComponentModel.DataAnnotations;
using NullOps.DataContract.Request;

namespace NullOps.DataContract.Response.Auth;

public class LoginResponse
{
    [Required]
    public required string Token { get; set; }
}
