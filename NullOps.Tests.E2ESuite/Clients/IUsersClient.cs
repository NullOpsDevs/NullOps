using NullOps.DataContract;
using NullOps.DataContract.Request.Auth;
using Refit;

namespace NullOps.Tests.E2ESuite.Clients;

public interface IUsersClient
{
    [Post("/api/v1/users/register")]
    public Task<IApiResponse<BaseResponse>> RegisterAsync([Body] RegisterRequest request, CancellationToken cancellationToken = default);
}