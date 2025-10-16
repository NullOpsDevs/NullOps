using System.Net;
using NullOps.DataContract.Request.Auth;
using NullOps.Services.Seeding.Seeders;
using NullOps.Tests.E2ESuite.Clients;
using Refit;

namespace NullOps.Tests.E2ESuite.Scenarios;

public class AuthenticationScenario : Scenario<GlobalTestContext>
{
    public AuthenticationScenario() : base("Initial authentication")
    {
        AddStep("Not authenticated when token is not passed", NotAuthenticatedWithoutToken);
        AddStep("Test suite is able to authenticate as admin", TestSuiteCanAuthenticateAsAdmin);
        AddStep("Test login not exist user", TestLoginNotExistUser);
        AddStep("Token refreshing works as admin", TokenRefreshingWorks);
        AddStep("Fake token refreshing works", FakeTokenRefreshingWorks);
    }
    
    private static async Task NotAuthenticatedWithoutToken(GlobalTestContext ctx)
    {
        var response = await ctx.AuthClient.CheckAuthAsync();
        
        Assert.ExpectStatusCode(response, HttpStatusCode.Unauthorized);
    }
    
    private static async Task TestSuiteCanAuthenticateAsAdmin(GlobalTestContext ctx)
    {
        var response = await ctx.AuthClient.LoginAsync(new LoginRequest
        {
            Username = SuperAdminSeederService.SuperAdminCredentials,
            Password = SuperAdminSeederService.SuperAdminCredentials
        });
        
        Assert.ExpectStatusCode(response, HttpStatusCode.OK);
        Assert.ExpectTrue(response.Content?.Success, "Authentication failed - default admin credentials were changed?");
        Assert.IsMeaningfulString(response.Content?.Data?.Token, "Token is empty");
        
        ctx.Token = response.Content!.Data!.Token;
    }
    
    private static async Task TestLoginNotExistUser(GlobalTestContext ctx)
    {
        var response = await ctx.AuthClient.LoginAsync(new LoginRequest
        {
            Username = Guid.NewGuid().ToString(),
            Password = Guid.NewGuid().ToString()
        });
        
        Assert.ExpectStatusCode(response, HttpStatusCode.Unauthorized);
    }
    
    private static async Task TokenRefreshingWorks(GlobalTestContext ctx)
    {
        var response = await ctx.AuthClient.RefreshAsync(ctx.Token!);
        
        Assert.ExpectStatusCode(response, HttpStatusCode.OK);
        Assert.ExpectTrue(response.Content?.Success, "Token refresh failed");
        Assert.IsMeaningfulString(response.Content?.Data?.Token, "Token is empty");
    }
    
    private static async Task FakeTokenRefreshingWorks(GlobalTestContext ctx)
    {
        var response = await ctx.AuthClient.RefreshAsync(Guid.NewGuid().ToString());
        
        Assert.ExpectStatusCode(response, HttpStatusCode.Unauthorized);
    }
}