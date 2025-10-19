using System.Collections.Frozen;
using NullOps.PipelinesExperiment.Components;
using NullOps.PipelinesExperiment.Configuration;

namespace NullOps.PipelinesExperiment.Validation;

public class PipelinePartContext
{
    public List<PipelineValidationEntry> ValidationEntries { get; init; }
    
    public PipelinePart Part { get; init; }
    
    public RegisteredPipelineComponent? RootComponent { get; set; }
    
    public RegisteredPipelineComponent[] KnownComponents { get; init; }
    public FrozenDictionary<string, RegisteredPipelineComponent> KnownComponentsMap { get; init; }
    
    public PipelineComponentConfiguration[] ComponentsConfigurations { get; init; }
    public FrozenDictionary<string, PipelineComponentConfiguration> ComponentsConfigurationsMap { get; init; }
    
    public HashSet<string> ReferencedComponentIds { get; init; }

    public bool HasErrors => ValidationEntries.Any(x => x.IsError);

    public void AddWarning(string? stepId, PipelineValidationEntryMessage message)
    {
        ValidationEntries.Add(new PipelineValidationEntry
        {
            StepId = stepId,
            Part = Part,
            Message = message,
            IsError = false
        });
    }

    public void AddError(string? stepId, PipelineValidationEntryMessage message)
    {
        ValidationEntries.Add(new PipelineValidationEntry
        {
            StepId = stepId,
            Part = Part,
            Message = message,
            IsError = true
        });   
    }
}