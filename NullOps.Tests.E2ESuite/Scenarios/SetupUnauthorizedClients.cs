using NullOps.Tests.E2ESuite.Clients;
using Refit;

namespace NullOps.Tests.E2ESuite.Scenarios;

public class SetupUnauthorizedClients : Scenario<GlobalTestContext>
{
    public SetupUnauthorizedClients() : base("Setup API unauthorized clients")
    {
        AddStep("Setting up API unauthorized clients for next scenarios", SetUnauthorizedClientsForNextScenarios);
    }
    
    private static Task SetUnauthorizedClientsForNextScenarios(GlobalTestContext ctx)
    {
        var refitSettings = new RefitSettings
        {
            AuthorizationHeaderValueGetter = (_, _) => Task.FromResult(Guid.NewGuid().ToString())
        };
        
        ctx.UsersClient = RestService.For<IUsersClient>(ctx.BaseUrl, refitSettings);
        
        return Task.CompletedTask;
    }
}