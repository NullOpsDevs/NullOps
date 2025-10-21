using NullOps.PipelinesExperimentV3.Components;
using NullOps.PipelinesExperimentV3.Configuration;
using NullOps.PipelinesExperimentV3.Validation;

namespace NullOps.PipelinesExperimentV3.Tests;

public class PipelineStepsTests
{
    private const string StartEventName = "start";
    
    [Test]
    public void Returns_validation_error_on_duplicating_step_ids()
    {
        var stepId = Guid.NewGuid();
        
        var componentRegistry = BuildComponentRegistry();
        
        var pipeline = new PipelineConfiguration
        {
            Steps = [
                new PipelineStepConfiguration { StepId = stepId },
                new PipelineStepConfiguration { StepId = stepId },
                new PipelineStepConfiguration { StepId = stepId }
            ]
        };

        var result = PipelineValidator.Validate(componentRegistry, pipeline);
        
        Assert.That(result.Pipeline, Is.Null);
        Assert.That(result.HasErrors, Is.True);
        Assert.That(result.ValidationEntries, Has.Count.EqualTo(1));
        Assert.That(result.ValidationEntries[0].Message, Is.EqualTo(PipelineValidationEntryMessage.StepIdsAreNotUnique));
    }

    [Test]
    public void Returns_validation_error_on_referencing_unknown_component()
    {
        var componentRegistry = BuildComponentRegistry();

        var pipeline = new PipelineConfiguration
        {
            Steps = [
                new PipelineStepConfiguration
                {
                    Dependencies = []
                }
            ]
        };

        var result = PipelineValidator.Validate(componentRegistry, pipeline);

        Assert.That(result.Pipeline, Is.Null);
        Assert.That(result.HasErrors, Is.True);
        Assert.That(result.ValidationEntries, Has.Count.EqualTo(1));
        Assert.That(result.ValidationEntries[0].Message, Is.EqualTo(PipelineValidationEntryMessage.UnknownComponentId));
    }

    [Test]
    public void Returns_validation_error_when_dependency_references_non_existent_step()
    {
        var componentRegistry = BuildComponentRegistry();
        var startEventId = componentRegistry.GetComponent(StartEventName)!.ComponentId;
        var testComponentId = componentRegistry.GetComponent("test")!.ComponentId;

        var nonExistentStepId = Guid.NewGuid();

        var pipeline = new PipelineConfiguration
        {
            Steps = [
                new PipelineStepConfiguration
                {
                    StepId = Guid.NewGuid(),
                    ComponentId = startEventId,
                    Dependencies = []
                },
                new PipelineStepConfiguration
                {
                    StepId = Guid.NewGuid(),
                    ComponentId = testComponentId,
                    Dependencies = [nonExistentStepId]
                }
            ]
        };

        var result = PipelineValidator.Validate(componentRegistry, pipeline);

        Assert.That(result.Pipeline, Is.Null);
        Assert.That(result.HasErrors, Is.True);
        Assert.That(result.ValidationEntries, Has.Count.EqualTo(1));
        Assert.That(result.ValidationEntries[0].Message, Is.EqualTo(PipelineValidationEntryMessage.DependencyReferencesNonExistentStep));
    }

    [Test]
    public void Returns_validation_error_when_multiple_dependencies_reference_non_existent_steps()
    {
        var componentRegistry = BuildComponentRegistry();
        var startEventId = componentRegistry.GetComponent(StartEventName)!.ComponentId;
        var testComponentId = componentRegistry.GetComponent("test")!.ComponentId;

        var nonExistentStepId1 = Guid.NewGuid();
        var nonExistentStepId2 = Guid.NewGuid();

        var pipeline = new PipelineConfiguration
        {
            Steps = [
                new PipelineStepConfiguration
                {
                    StepId = Guid.NewGuid(),
                    ComponentId = startEventId,
                    Dependencies = []
                },
                new PipelineStepConfiguration
                {
                    StepId = Guid.NewGuid(),
                    ComponentId = testComponentId,
                    Dependencies = [nonExistentStepId1, nonExistentStepId2]
                }
            ]
        };

        var result = PipelineValidator.Validate(componentRegistry, pipeline);

        Assert.That(result.Pipeline, Is.Null);
        Assert.That(result.HasErrors, Is.True);
        Assert.That(result.ValidationEntries, Has.Count.EqualTo(2));
        Assert.That(result.ValidationEntries.All(e => e.Message == PipelineValidationEntryMessage.DependencyReferencesNonExistentStep), Is.True);
    }

    [Test]
    public void Returns_validation_error_when_multiple_root_steps_exist()
    {
        var componentRegistry = BuildComponentRegistry();
        var startEventId = componentRegistry.GetComponent(StartEventName)!.ComponentId;

        var pipeline = new PipelineConfiguration
        {
            Steps = [
                new PipelineStepConfiguration { StepId = Guid.NewGuid(), ComponentId = startEventId, Dependencies = [] },
                new PipelineStepConfiguration { StepId = Guid.NewGuid(), ComponentId = startEventId, Dependencies = [] }
            ]
        };

        var result = PipelineValidator.Validate(componentRegistry, pipeline);

        Assert.That(result.Pipeline, Is.Null);
        Assert.That(result.HasErrors, Is.True);
        Assert.That(result.ValidationEntries, Has.Count.EqualTo(2));
        Assert.That(result.ValidationEntries[0].Message, Is.EqualTo(PipelineValidationEntryMessage.RootStepMustBeUnique));
        Assert.That(result.ValidationEntries[1].Message, Is.EqualTo(PipelineValidationEntryMessage.RootStepMustBeUnique));
    }

    [Test]
    public void Returns_validation_error_when_root_step_has_dependencies()
    {
        var componentRegistry = BuildComponentRegistry();
        var startEventId = componentRegistry.GetComponent(StartEventName)!.ComponentId;
        var testComponentId = componentRegistry.GetComponent("test")!.ComponentId;

        var dependencyStepId = Guid.NewGuid();

        var pipeline = new PipelineConfiguration
        {
            Steps = [
                new PipelineStepConfiguration
                {
                    StepId = Guid.NewGuid(),
                    ComponentId = startEventId,
                    Dependencies = [dependencyStepId]
                },
                new PipelineStepConfiguration
                {
                    StepId = dependencyStepId,
                    ComponentId = testComponentId,
                    Dependencies = []
                }
            ]
        };

        var result = PipelineValidator.Validate(componentRegistry, pipeline);

        Assert.That(result.Pipeline, Is.Null);
        Assert.That(result.HasErrors, Is.True);
        Assert.That(result.ValidationEntries, Has.Count.EqualTo(1));
        Assert.That(result.ValidationEntries[0].Message, Is.EqualTo(PipelineValidationEntryMessage.RootStepMustNotHaveDependencies));
    }

    [Test]
    public void Returns_validation_error_when_cyclic_dependency_detected()
    {
        var componentRegistry = BuildComponentRegistry();
        var testComponentId = componentRegistry.GetComponent("test")!.ComponentId;
        var startEventId = componentRegistry.GetComponent(StartEventName)!.ComponentId;

        var step1Id = Guid.NewGuid();
        var step2Id = Guid.NewGuid();
        var step3Id = Guid.NewGuid();

        var pipeline = new PipelineConfiguration
        {
            Steps = [
                new PipelineStepConfiguration
                {
                    StepId = Guid.NewGuid(),
                    ComponentId = startEventId,
                    Dependencies = []
                },
                new PipelineStepConfiguration
                {
                    StepId = step1Id,
                    ComponentId = testComponentId,
                    Dependencies = [step3Id] // Creates cycle: step1 -> step3 -> step2 -> step1
                },
                new PipelineStepConfiguration
                {
                    StepId = step2Id,
                    ComponentId = testComponentId,
                    Dependencies = [step1Id]
                },
                new PipelineStepConfiguration
                {
                    StepId = step3Id,
                    ComponentId = testComponentId,
                    Dependencies = [step2Id]
                }
            ]
        };

        var result = PipelineValidator.Validate(componentRegistry, pipeline);

        Assert.That(result.Pipeline, Is.Null);
        Assert.That(result.HasErrors, Is.True);
        Assert.That(result.ValidationEntries, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(result.ValidationEntries[0].Message, Is.EqualTo(PipelineValidationEntryMessage.CyclicDependencyDetected));
    }

    [Test]
    public void Returns_validation_warning_for_dangling_steps()
    {
        var componentRegistry = BuildComponentRegistry();
        var startEventId = componentRegistry.GetComponent(StartEventName)!.ComponentId;
        var testComponentId = componentRegistry.GetComponent("test")!.ComponentId;

        var startStepId = Guid.NewGuid();
        var connectedStepId = Guid.NewGuid();
        var danglingStepId = Guid.NewGuid();
        var anotherDanglingStepId = Guid.NewGuid();

        var pipeline = new PipelineConfiguration
        {
            Steps = [
                new PipelineStepConfiguration
                {
                    StepId = startStepId,
                    ComponentId = startEventId,
                    Dependencies = []
                },
                new PipelineStepConfiguration
                {
                    StepId = connectedStepId,
                    ComponentId = testComponentId,
                    Dependencies = [startStepId]
                },
                new PipelineStepConfiguration
                {
                    StepId = danglingStepId,
                    ComponentId = testComponentId,
                    Dependencies = [] // Depends on no-one
                },
                new PipelineStepConfiguration
                {
                    StepId = anotherDanglingStepId,
                    ComponentId = testComponentId,
                    Dependencies = [danglingStepId] // Depends on dangling step
                }
            ]
        };

        var result = PipelineValidator.Validate(componentRegistry, pipeline);

        Assert.That(result.Pipeline, Is.Null);
        Assert.That(result.HasWarnings, Is.True);
        Assert.That(result.ValidationEntries, Has.Count.EqualTo(2));
        
        var danglingStepIds = new HashSet<Guid> { danglingStepId, anotherDanglingStepId };
        
        var receivedDanglingStepIds = result.ValidationEntries
            .Where(e => e.Message == PipelineValidationEntryMessage.DanglingStep)
            .Select(e => e.StepId!.Value)
            .ToHashSet();
        
        var intersection = danglingStepIds.Intersect(receivedDanglingStepIds).ToArray();
        
        Assert.That(intersection, Has.Length.EqualTo(2));
    }

    [Test]
    public void Validates_successfully_with_valid_pipeline()
    {
        var componentRegistry = BuildComponentRegistry();
        var startEventId = componentRegistry.GetComponent(StartEventName)!.ComponentId;
        var testComponentId = componentRegistry.GetComponent("test")!.ComponentId;

        var step1Id = Guid.NewGuid();
        var step2Id = Guid.NewGuid();
        var step3Id = Guid.NewGuid();

        var pipeline = new PipelineConfiguration
        {
            Steps = [
                new PipelineStepConfiguration
                {
                    StepId = step1Id,
                    ComponentId = startEventId,
                    Dependencies = []
                },
                new PipelineStepConfiguration
                {
                    StepId = step2Id,
                    ComponentId = testComponentId,
                    Dependencies = [step1Id]
                },
                new PipelineStepConfiguration
                {
                    StepId = step3Id,
                    ComponentId = testComponentId,
                    Dependencies = [step2Id]
                }
            ]
        };

        var result = PipelineValidator.Validate(componentRegistry, pipeline);

        Assert.That(result.HasErrors, Is.False);
        Assert.That(result.HasWarnings, Is.False);
        Assert.That(result.ValidationEntries, Is.Empty);
    }

    private static ComponentRegistry BuildComponentRegistry()
    {
        var cr = new ComponentRegistry();

        cr.AddComponent(new RegisteredPipelineComponent
        {
            ComponentId = Guid.NewGuid(),
            Type = RegisteredPipelineComponentType.Internal,
            Name = StartEventName,
            Description = string.Empty,
            IsSystemEvent = true
        });

        cr.AddComponent(new RegisteredPipelineComponent
        {
            ComponentId = Guid.NewGuid(),
            Type = RegisteredPipelineComponentType.Internal,
            Name = "test",
            Description = string.Empty,
            IsSystemEvent = false
        });

        return cr;
    }
}