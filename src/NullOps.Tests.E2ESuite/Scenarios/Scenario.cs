using NullOps.Tests.E2ESuite.Exceptions;
using Spectre.Console;

namespace NullOps.Tests.E2ESuite.Scenarios;

public abstract class Scenario<TContext>(string name)
    where TContext : class
{
    public string Name => name;
    
    private readonly Queue<ScenarioStep<TContext>> steps = new();

    public void AddStep(string description, Func<TContext, Task> action)
    {
        steps.Enqueue(new ScenarioStep<TContext>(description, action));
    }
    
    public async Task<bool> RunScenario(TContext context)
    {
        AnsiConsole.MarkupLine($"Running scenario '[blue]{name}[/]'");
        
        foreach (var step in steps)
        {
            var status = await AnsiConsole.Status()
                .StartAsync(
                    $"[blue]{Markup.Escape(step.Description)}[/]",
                    async _ => await step.RunAsync(context));
            
            if (!status.Success)
            {
                if (status.Exception is TestException testException)
                {
                    AnsiConsole.MarkupLine($"❌  [underline]{Markup.Escape(step.Description)}[/] - [red]FAIL[/] - {Markup.Escape(testException.Message)}");
                }
                else
                {
                    AnsiConsole.MarkupLine($"❌  [underline]{Markup.Escape(step.Description)}[/] - [red]UNHANDLED EXCEPTION[/]");
                    AnsiConsole.WriteException(status.Exception!);
                }

                return false;
            }
            
            AnsiConsole.MarkupLine($"✅  [underline]{Markup.Escape(step.Description)}[/] - [green]PASS[/]");
        }
        
        return true;
    }
}