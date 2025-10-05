namespace NullOps.Tests.E2ESuite.Scenarios;

public class ScenarioStep<TContext>(string description, Func<TContext, Task> action)
{
    public string Description => description;
    
    public async Task<StepExecutionResult> RunAsync(TContext context)
    {
        try
        {
            await action(context);
            return new StepExecutionResult(true);
        }
        catch (Exception ex)
        {
            return new StepExecutionResult(false, ex);
        }
    }
}