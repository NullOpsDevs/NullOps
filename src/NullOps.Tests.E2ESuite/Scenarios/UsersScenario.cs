using System.Net;
using NullOps.Tests.E2ESuite.Extensions;

namespace NullOps.Tests.E2ESuite.Scenarios;

public class UsersScenario : Scenario<GlobalTestContext>
{
    public UsersScenario() : base("Users")
    {
        AddStep("User is able to retrieve their own profile", UserIsAbleToRetrieveOwnProfile);
        AddStep("Admin is able to retrieve user list", AdminIsAbleToRetrieveUserList);
    }

    private static async Task UserIsAbleToRetrieveOwnProfile(GlobalTestContext ctx)
    {
        var response = await ctx.UsersClient.GetMeAsync();
        
        Assert.ExpectStatusCode(response, HttpStatusCode.OK);

        var content = response.GetContent();
        
        Assert.IsNotNull(content, "Response content is null");
    }

    private static async Task AdminIsAbleToRetrieveUserList(GlobalTestContext ctx)
    {
        var response = await ctx.UsersClient.GetUsersAsync();
        
        Assert.ExpectStatusCode(response, HttpStatusCode.OK);

        var content = response.GetContent();
        
        Assert.IsNotNull(content, "Response content is null");
        Assert.IsNotNull(content?.Data, "No users returned");
        Assert.Must(content?.Data?.Count(), count => count > 0, "No users returned");
    }
}