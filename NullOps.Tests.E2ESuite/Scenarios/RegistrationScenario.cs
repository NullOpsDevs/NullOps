using System.Net;
using NullOps.DataContract.Request.Auth;
using NullOps.Tests.E2ESuite.Extensions;

namespace NullOps.Tests.E2ESuite.Scenarios;

public class RegistrationScenario : Scenario<GlobalTestContext>
{
    private const string TestUsername = "e2e-test-user";
    
    public RegistrationScenario() : base("Registration")
    {
        AddStep("Server will register new user when registration is enabled", ServerWillRegisterNewUser);
        AddStep("Server will refuse to register user with same username", ServerWillRefuseToCreateKnownUser);
        AddStep("Server will refuse to register a user when registration is disabled", ServerWillRefuseWhenRegistrationIsDisabled);
    }

    private static async Task ServerWillRegisterNewUser(GlobalTestContext ctx)
    {
        await ctx.TestSuiteClient.SetSettingAsync(Settings.RegistrationEnabled, "true");
        
        var response = await ctx.UsersClient.RegisterAsync(new RegisterRequest
        {
            Username = TestUsername,
            Password = "123456"
        });
        
        Assert.ExpectStatusCode(response, HttpStatusCode.Created);
        Assert.ExpectTrue(response.GetContent()?.Success, "Registration failed");
    }
    
    private static async Task ServerWillRefuseToCreateKnownUser(GlobalTestContext ctx)
    {
        await ctx.TestSuiteClient.SetSettingAsync(Settings.RegistrationEnabled, "true");
        
        var response = await ctx.UsersClient.RegisterAsync(new RegisterRequest
        {
            Username = TestUsername,
            Password = "123456"
        });
        
        Assert.ExpectStatusCode(response, HttpStatusCode.Conflict);
        Assert.ExpectFalse(response.GetContent()?.Success, "Duplicate user was registered");
    }
    
    private static async Task ServerWillRefuseWhenRegistrationIsDisabled(GlobalTestContext ctx)
    {
        await ctx.TestSuiteClient.SetSettingAsync(Settings.RegistrationEnabled, "false");
        
        var response = await ctx.UsersClient.RegisterAsync(new RegisterRequest
        {
            Username = TestUsername,
            Password = "123456"
        });
        
        Assert.ExpectStatusCode(response, HttpStatusCode.Forbidden);
        Assert.ExpectFalse(response.GetContent()?.Success, "Duplicate user was registered");
    }
}