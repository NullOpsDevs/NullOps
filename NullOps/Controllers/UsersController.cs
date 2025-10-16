using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NullOps.DAL.Enums;
using NullOps.DataContract;
using NullOps.DataContract.Request;
using NullOps.DataContract.Request.Users;
using NullOps.DataContract.Response.Users;
using NullOps.Extensions;
using NullOps.Services.Users;

namespace NullOps.Controllers;

[Controller]
[Route("/api/v1/users")]
public class RegistrationController(UsersService usersService) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<PagedResponse<UserDto>> GetUsersAsync(
        [AsParameters] Paging paging,
        CancellationToken cancellationToken = default)
    {
        User.AssertIsAdministrator();

        return await usersService.GetUsersAsync(paging, cancellationToken);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<BaseResponse<UserDto>> GetMeAsync(CancellationToken cancellationToken = default)
    {
        return BaseResponse<UserDto>
            .CreateSuccessful()
            .WithData(await usersService.GetMeAsync(User.GetUserId(), cancellationToken));
    }
    
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
