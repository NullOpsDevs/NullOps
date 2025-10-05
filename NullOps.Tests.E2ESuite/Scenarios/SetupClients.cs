using NullOps.Tests.E2ESuite.Clients;
using Refit;

namespace NullOps.Tests.E2ESuite.Scenarios;

public class SetupClients : Scenario<GlobalTestContext>
{
    public SetupClients() : base("Setup API clients")
    {
        AddStep("Setting up API clients for next scenarios", SetUpClientsForNextScenarios);
    }

    private static Task SetUpClientsForNextScenarios(GlobalTestContext ctx)
    {
        var refitSettings = new RefitSettings
        {
            AuthorizationHeaderValueGetter = (_, _) => Task.FromResult(ctx.Token)!
        };
        
        ctx.UsersClient = RestService.For<IUsersClient>(ctx.BaseUrl, refitSettings);
        
        return Task.CompletedTask;
    }
}