using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NullOps.DataContract;
using NullOps.DataContract.Request.Auth;
using NullOps.DataContract.Response.Auth;
using NullOps.Extensions;
using NullOps.Services.Users;

namespace NullOps.Controllers;

[Controller]
[Route("/api/v1/auth")]
public class AuthController(UsersService usersService) : ControllerBase
{
    [Authorize]
    [HttpGet("status")]
    public BaseResponse CheckAuth()
    {
        return BaseResponse.Successful;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<BaseResponse<LoginResponse>> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        request.ValidateOrThrow();
        
        var token = await usersService.LoginAsync(request.Username, request.Password, cancellationToken);

        return BaseResponse<LoginResponse>
            .CreateSuccessful()
            .WithData(new LoginResponse
            {
                Token = token
            });
    }

    [Authorize]
    [HttpPost("refresh")]
    public async Task<BaseResponse<LoginResponse>> RefreshAsync(CancellationToken cancellationToken)
    {
        var token = await usersService.UpdateTokenAsync(User.GetUserId(), cancellationToken);
        
        return BaseResponse<LoginResponse>
            .CreateSuccessful()
            .WithData(new LoginResponse
            {
                Token = token
            });
    }
}