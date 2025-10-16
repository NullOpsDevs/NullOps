using System.Net;

namespace NullOps.Tests.E2ESuite.Scenarios;

public class UnauthorizedUsersScenario : Scenario<GlobalTestContext>
{
    public UnauthorizedUsersScenario() : base("Verify unauthorized")
    {
        AddStep("Verify unauthorized get me", VerifyUnauthorizedGetMe);
        AddStep("Verify unauthorized get users", VerifyUnauthorizedGetUsers);
    }
    
    private static async Task VerifyUnauthorizedGetMe(GlobalTestContext ctx)
    {
        var response = await ctx.UsersClient.GetMeAsync();
        
        Assert.ExpectStatusCode(response, HttpStatusCode.Unauthorized);
    }
    
    private static async Task VerifyUnauthorizedGetUsers(GlobalTestContext ctx)
    {
        var response = await ctx.UsersClient.GetUsersAsync();
        
        Assert.ExpectStatusCode(response, HttpStatusCode.Unauthorized);
    }
}