using NullOps.DataContract;
using Refit;

namespace NullOps.Tests.E2ESuite.Clients;

public interface ITestSuiteClient
{
    [Get("/api/v1/test-suite/clear-database")]
    public Task<IApiResponse<BaseResponse>> ClearDatabaseAsync();
}