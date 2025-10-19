using System.Collections.Frozen;
using NullOps.PipelinesExperiment.Components;
using NullOps.PipelinesExperiment.Configuration;

namespace NullOps.PipelinesExperiment.Validation;

public class PipelineForestBuilder
{
    public static PipelineForest Build(
        RegisteredPipelineComponent[] registeredComponents,
        PipelineComponentConfiguration[] startComponents,
        PipelineComponentConfiguration[] succeededComponents,
        PipelineComponentConfiguration[] failedComponents,
        PipelineComponentConfiguration[] cancelledComponents)
    {
        // "Start" of the pipeline is actually the only required part of the forest
        // since it controls the pipeline root behavior and can't be empty
        // if it contains only 1 element (root one) - it's still not a valid, empty pipeline
        if (startComponents.Length is 0 or 1)
            return PipelineForest.FromSingleError(null, PipelinePart.Start, PipelineValidationEntryMessage.StartOfPipelineIsEmpty);

        var allValidationEntries = new List<PipelineValidationEntry>();
        
        var startContext = PrepareContext(PipelinePart.Start, registeredComponents, startComponents);
        var startPipelineTree = BuildPipelinePart(PipelinePart.Start, startContext);
        
        if(startPipelineTree == null)
            allValidationEntries.AddRange(startContext.ValidationEntries);

        return new PipelineForest
        {
            PipelineStart = startPipelineTree,
            
            ValidationEntries = allValidationEntries.ToArray()
        };
    }

    public static PipelineNode? BuildPipelinePart(PipelinePart part, PipelinePartContext context)
    {
        if (!Verify_that_root_component_was_found(context))
            return null;
        
        if (!Verify_all_configurations_reference_existing_components(context))
            return null;
        
        
    }

    private static PipelinePartContext PrepareContext(PipelinePart part, RegisteredPipelineComponent[] components, PipelineComponentConfiguration[] componentConfigurations)
    {
        var startComponent = part switch
        {
            PipelinePart.Start => components.FirstOrDefault(x => x.ComponentId == PipelineConsts.PipelineStart),
            PipelinePart.Succeeded => components.FirstOrDefault(x => x.ComponentId == PipelineConsts.PipelineSucceeded),
            PipelinePart.Failed => components.FirstOrDefault(x => x.ComponentId == PipelineConsts.PipelineFailed),
            PipelinePart.Cancelled => components.FirstOrDefault(x => x.ComponentId == PipelineConsts.PipelineCancelled),
            _ => null
        };
        
        return new PipelinePartContext
        {
            ValidationEntries = [],
            
            Part = part,
            
            RootComponent = startComponent,
            
            KnownComponents = components,
            KnownComponentsMap = components.ToFrozenDictionary(x => x.ComponentId),
            
            ComponentsConfigurations = componentConfigurations,
            ComponentsConfigurationsMap = componentConfigurations.ToFrozenDictionary(x => x.ComponentId),
            
            ReferencedComponentIds = componentConfigurations.Select(x => x.ComponentId).ToHashSet()
        };
    }

    private static bool Verify_that_root_component_was_found(PipelinePartContext context)
    {
        if (context.RootComponent != null)
            return true;
        
        context.AddError(null, PipelineValidationEntryMessage.RootComponentIsEmpty);
        return false;
    }
    
    private static bool Verify_all_configurations_reference_existing_components(PipelinePartContext context)
    {
        foreach (var componentConfig in context.ComponentsConfigurations)
        {
            if (!context.KnownComponentsMap.ContainsKey(componentConfig.ComponentId)) 
                context.AddError(componentConfig.StepId, PipelineValidationEntryMessage.StepReferencesUnknownComponentId);
        }
        
        return !context.HasErrors;
    }
}