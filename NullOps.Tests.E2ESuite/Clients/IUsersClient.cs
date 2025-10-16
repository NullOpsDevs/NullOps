using NullOps.DataContract;
using NullOps.DataContract.Request.Users;
using NullOps.DataContract.Response.Users;
using Refit;

namespace NullOps.Tests.E2ESuite.Clients;

[Headers("Authorization: Bearer")]
public interface IUsersClient
{
    [Get("/api/v1/users/")]
    public Task<IApiResponse<PagedResponse<UserDto>>> GetUsersAsync(CancellationToken cancellationToken = default);
    
    [Get("/api/v1/users/me")]
    public Task<IApiResponse<BaseResponse<UserDto>>> GetMeAsync(CancellationToken cancellationToken = default);
    
    [Post("/api/v1/users/register")]
    public Task<IApiResponse<BaseResponse>> RegisterAsync([Body] RegisterRequest request, CancellationToken cancellationToken = default);
}