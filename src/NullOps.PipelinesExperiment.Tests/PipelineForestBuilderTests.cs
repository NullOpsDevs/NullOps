using NullOps.PipelinesExperiment.Components;
using NullOps.PipelinesExperiment.Configuration;
using NullOps.PipelinesExperiment.Validation;

namespace NullOps.PipelinesExperiment.Tests;

public class PipelineForestBuilderTests
{
    private const string ExampleComponentId = "example";

    private const string SimpleStartStepId = "start-step";
    private const string SimpleExampleStepId = "example-step";
    private const string SimpleInnerExampleStepId = "inner-example-step";
    
    [Test]
    [TestCaseSource(nameof(Builds_simple_pipeline_source))]
    public void Builds_simple_pipeline(
        string type,
        PipelineComponentConfiguration[] configurations)
    {
        // This is a positive scenario test
        // We are testing that the pipeline forest builder works correctly on a simple example
        
        var registeredComponents = GetRegisteredComponents();

        var forest = PipelineForestBuilder.Build(
            registeredComponents:  registeredComponents,
            startComponents:       configurations,
            succeededComponents:   GetSingleComponentConfiguration(PipelineConsts.PipelineSucceeded),
            failedComponents:      GetSingleComponentConfiguration(PipelineConsts.PipelineFailed),
            cancelledComponents:   GetSingleComponentConfiguration(PipelineConsts.PipelineCancelled));

        Assert.That(forest.PipelineStart, Is.Not.Null);
        Assert.That(forest.PipelineStart.StepId, Is.EqualTo(SimpleStartStepId));
        
        Assert.That(forest.PipelineStart.NextNodes, Is.Not.Null);
        Assert.That(forest.PipelineStart.NextNodes.Count, Is.EqualTo(1));
        Assert.That(forest.PipelineStart.NextNodes[0].StepId, Is.EqualTo(SimpleExampleStepId));
        
        Assert.That(forest.PipelineStart.NextNodes[0].NextNodes, Is.Not.Null);
        Assert.That(forest.PipelineStart.NextNodes[0].NextNodes.Count, Is.EqualTo(1));
        Assert.That(forest.PipelineStart.NextNodes[0].NextNodes[0].StepId, Is.EqualTo(SimpleInnerExampleStepId));
    }

    private static IEnumerable<object[]> Builds_simple_pipeline_source()
    {
        var startComponent = new PipelineComponentConfiguration
        {
            StepId = SimpleStartStepId,
            ComponentId = PipelineConsts.PipelineStart,
            Parameters = [],
            NextComponents = [SimpleExampleStepId]
        };

        var exampleComponent = new PipelineComponentConfiguration
        {
            StepId = SimpleExampleStepId,
            ComponentId = ExampleComponentId,
            Parameters = [],
            NextComponents = [SimpleInnerExampleStepId]
        };

        var innerExampleComponent = new PipelineComponentConfiguration
        {
            StepId = SimpleInnerExampleStepId,
            ComponentId = ExampleComponentId,
            Parameters = [],
            NextComponents = []
        };
        
        yield return
        [
            "Ordered components",
            new[] { startComponent, exampleComponent, innerExampleComponent }
        ];

        yield return
        [
            "Inversed components",
            new[] { innerExampleComponent, exampleComponent, startComponent }
        ];
        
        yield return
        [
            "Mixed components",
            new[] { innerExampleComponent, exampleComponent, startComponent }
        ];
        
        yield return
        [
            "Mixed components 2",
            new[] { innerExampleComponent, startComponent, exampleComponent }
        ];
    }

    private static RegisteredPipelineComponent[] GetRegisteredComponents()
    {
        return [
            new RegisteredPipelineComponent
            {
                ComponentId = PipelineConsts.PipelineStart,
                Type = RegisteredPipelineComponentType.Internal,
                Name = "Start",
                Description = string.Empty
            },
            new RegisteredPipelineComponent
            {
                ComponentId = PipelineConsts.PipelineSucceeded,
                Type = RegisteredPipelineComponentType.Internal,
                Name = "Succeeded",
                Description = string.Empty
            },
            new RegisteredPipelineComponent
            {
                ComponentId = PipelineConsts.PipelineFailed,
                Type = RegisteredPipelineComponentType.Internal,
                Name = "Failed",
                Description = string.Empty
            },
            new RegisteredPipelineComponent
            {
                ComponentId = PipelineConsts.PipelineCancelled,
                Type = RegisteredPipelineComponentType.Internal,
                Name = "Cancelled",
                Description = string.Empty
            },
            new RegisteredPipelineComponent
            {
                ComponentId = ExampleComponentId,
                Type = RegisteredPipelineComponentType.Internal,
                Name = "Example",
                Description = string.Empty
            }
        ];
    }
    
    private static PipelineComponentConfiguration[] GetSingleComponentConfiguration(string componentId)
    {
        return
        [
            new PipelineComponentConfiguration
            {
                StepId = Guid.NewGuid().ToString(),
                ComponentId = componentId,
                Parameters = [],
                NextComponents = []
            }
        ];
    }
}