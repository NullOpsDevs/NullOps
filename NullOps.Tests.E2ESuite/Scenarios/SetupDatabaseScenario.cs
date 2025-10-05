using System.Net;

namespace NullOps.Tests.E2ESuite.Scenarios;

public class SetupDatabaseScenario : Scenario<GlobalTestContext>
{
    public SetupDatabaseScenario() : base("Setup database")
    {
        AddStep("Wipe database and setup superadmin", SetupDatabase);
    }

    private static async Task SetupDatabase(GlobalTestContext ctx)
    {
        var response = await ctx.TestSuiteClient.ClearDatabaseAsync();
        
        Assert.ExpectStatusCode(response, HttpStatusCode.OK);
        Assert.IsTrue(response.Content?.Success, "Database wipe failed - development mode is not enabled?");
    }
}