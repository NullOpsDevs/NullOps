using NullOps.DataContract;
using NullOps.DataContract.Request.Auth;
using NullOps.DataContract.Response.Auth;
using Refit;

namespace NullOps.Tests.E2ESuite.Clients;

public interface IAuthClient
{
    [Get("/api/v1/auth/status")]
    Task<IApiResponse<BaseResponse>> CheckAuthAsync(CancellationToken cancellationToken = default);
    
    [Post("/api/v1/auth/login")]
    Task<IApiResponse<BaseResponse<LoginResponse>>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    
    [Post("/api/v1/auth/refresh")]
    Task<IApiResponse<BaseResponse<LoginResponse>>> RefreshAsync(
        [Authorize] string currentToken, CancellationToken cancellationToken = default);
}
