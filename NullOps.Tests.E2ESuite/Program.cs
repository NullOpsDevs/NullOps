using System.Text;
using NullOps.Tests.E2ESuite;
using NullOps.Tests.E2ESuite.Clients;
using NullOps.Tests.E2ESuite.Scenarios;
using Refit;
using Spectre.Console;

Console.OutputEncoding = Encoding.UTF8;

var baseUrl = new Uri("http://localhost:7000", UriKind.Absolute);
var overridingUrl = Environment.GetEnvironmentVariable("E2ESUITE_URL");

if (Uri.TryCreate(overridingUrl, UriKind.Absolute, out var url))
{
    AnsiConsole.MarkupLine($"[yellow]Overriding base URL with '{url}'[/]");
    baseUrl = url;
}

var globalContext = new GlobalTestContext
{
    AuthClient = RestService.For<IAuthClient>(baseUrl.ToString()),
    TestSuiteClient = RestService.For<ITestSuiteClient>(baseUrl.ToString())
};

var scenarios = new Scenario<GlobalTestContext>[]
{
    new SetupDatabaseScenario(),
    new AuthenticationScenario()
};

foreach (var scenario in scenarios)
{
    var result = await scenario.RunScenario(globalContext);

    if (!result)
    {
        AnsiConsole.MarkupLine($"[red]Scenario failed: '[underline]{scenario.Name}[/]'[/]");
        return 255;
    }
}

return 0;