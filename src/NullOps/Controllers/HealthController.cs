using Microsoft.AspNetCore.Mvc;
using NullOps.DataContract;

namespace NullOps.Controllers;

[Controller]
[Route("/api/v1/health")]
public class HealthController : ControllerBase
{
    [HttpGet("ping")]
    public BaseResponse Ping() => BaseResponse.Successful;
}
