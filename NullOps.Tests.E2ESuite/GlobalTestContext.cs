using NullOps.Tests.E2ESuite.Clients;

namespace NullOps.Tests.E2ESuite;

public class GlobalTestContext
{
    public required IAuthClient AuthClient { get; init; }
    public required ITestSuiteClient TestSuiteClient { get; init; }
    
    public string? Token { get; set; }
}
