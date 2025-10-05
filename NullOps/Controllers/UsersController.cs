using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NullOps.DAL.Enums;
using NullOps.DataContract;
using NullOps.DataContract.Request.Auth;
using NullOps.Services.Users;

namespace NullOps.Controllers;

[Controller]
[Route("/api/v1/users")]
public class RegistrationController(UsersService usersService) : Controller
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<BaseResponse> RegisterAsync([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        request.ValidateOrThrow();
        
        await usersService.RegisterUserAsync(request.Username, request.Password, UserRole.User, cancellationToken);
        
        HttpContext.Response.StatusCode = (int) HttpStatusCode.Created;
        return BaseResponse.Successful;
    }
}
