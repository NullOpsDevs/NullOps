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

    private static PipelineNode? BuildPipelinePart(PipelinePart part, PipelinePartContext context)
    {
        if (!Verify_that_root_component_was_found(context))
            return null;

        if (!Verify_all_configurations_reference_existing_components(context))
            return null;

        if (!Verify_all_step_ids_are_unique(context))
            return null;

        context.PostValidationConfigure();
        
        // Detect cycles using iterative DFS
        if (!DetectCycles(context))
            return null;

        // Build the tree iteratively using topological-like processing
        var builtNodes = BuildNodesIteratively(context);

        if (context.HasErrors)
            return null;

        // Return the root node
        return builtNodes.GetValueOrDefault(context.RootComponent!.StepId);
    }

    private static bool DetectCycles(PipelinePartContext context)
    {
        var visited = new HashSet<string>();
        var currentPath = new HashSet<string>();
        var stack = new Stack<(PipelineComponentConfiguration config, bool isReturning)>();

        stack.Push((context.RootComponent!, false));

        while (stack.Count > 0)
        {
            var (config, isReturning) = stack.Pop();

            if (isReturning)
            {
                // We're returning from processing children, remove from current path
                currentPath.Remove(config.StepId);
                continue;
            }

            // Check for cycle
            if (currentPath.Contains(config.StepId))
            {
                context.AddError(config.StepId, PipelineValidationEntryMessage.CycleDetectedInPipeline);
                return false;
            }

            // Skip if already fully processed
            if (visited.Contains(config.StepId))
                continue;

            visited.Add(config.StepId);
            currentPath.Add(config.StepId);

            // Push a marker to remove from path when we're done with children
            stack.Push((config, true));

            // Process children in reverse order (so they're processed in correct order from stack)
            if (config.NextComponents != null)
            {
                for (var i = config.NextComponents.Length - 1; i >= 0; i--)
                {
                    var nextStepId = config.NextComponents[i];
                    if (context.ComponentsConfigurationsByStepId.TryGetValue(nextStepId, out var nextConfig))
                    {
                        stack.Push((nextConfig, false));
                    }
                }
            }
        }

        return true;
    }

    private static Dictionary<string, PipelineNode> BuildNodesIteratively(PipelinePartContext context)
    {
        var builtNodes = new Dictionary<string, PipelineNode>();
        var childrenCount = new Dictionary<string, int>();
        var parents = new Dictionary<string, List<string>>();

        // Build parent-child relationships
        foreach (var config in context.ComponentsConfigurations)
        {
            childrenCount.TryAdd(config.StepId, 0);

            if (config.NextComponents != null)
            {
                childrenCount[config.StepId] = config.NextComponents.Length;

                foreach (var nextStepId in config.NextComponents)
                {
                    if (!parents.ContainsKey(nextStepId))
                        parents[nextStepId] = [];

                    parents[nextStepId].Add(config.StepId);
                }
            }
        }

        // Build nodes in reverse topological order (from leaves to root)
        var queue = new Queue<string>();

        // Start with leaf nodes (nodes with no children)
        foreach (var config in context.ComponentsConfigurations)
        {
            if (childrenCount[config.StepId] == 0)
            {
                queue.Enqueue(config.StepId);
            }
        }

        while (queue.Count > 0)
        {
            var stepId = queue.Dequeue();

            if (!context.ComponentsConfigurationsByStepId.TryGetValue(stepId, out var config))
                continue;

            // Build this node
            if (!context.KnownComponentsMap.TryGetValue(config.ComponentId, out var component))
            {
                context.AddError(config.StepId, PipelineValidationEntryMessage.StepReferencesUnknownComponentId);
                continue;
            }

            var nextNodes = new List<PipelineNode>();

            if (config.NextComponents != null)
            {
                foreach (var nextStepId in config.NextComponents)
                {
                    if (builtNodes.TryGetValue(nextStepId, out var childNode))
                    {
                        nextNodes.Add(childNode);
                    }
                }
            }

            var node = new PipelineNode
            {
                StepId = config.StepId,
                ComponentId = config.ComponentId,
                Component = component,
                Parameters = config.Parameters?.Select(p => new PipelineNodeConfigurationParameter
                {
                    Name = p.Name,
                    Expression = p.Expression
                }).ToArray() ?? [],
                NextNodes = nextNodes.ToArray()
            };

            builtNodes[config.StepId] = node;

            // Notify parents that this child has been built
            if (parents.TryGetValue(stepId, out var parentStepIds))
            {
                foreach (var parentStepId in parentStepIds)
                {
                    childrenCount[parentStepId]--;

                    // If all children of parent are built, add parent to queue
                    if (childrenCount[parentStepId] == 0)
                    {
                        queue.Enqueue(parentStepId);
                    }
                }
            }
        }

        return builtNodes;
    }

    private static PipelinePartContext PrepareContext(PipelinePart part, RegisteredPipelineComponent[] components, PipelineComponentConfiguration[] componentConfigurations)
    {
        var startComponent = part switch
        {
            PipelinePart.Start => componentConfigurations.FirstOrDefault(x => x.ComponentId == PipelineConsts.PipelineStart),
            PipelinePart.Succeeded => componentConfigurations.FirstOrDefault(x => x.ComponentId == PipelineConsts.PipelineSucceeded),
            PipelinePart.Failed => componentConfigurations.FirstOrDefault(x => x.ComponentId == PipelineConsts.PipelineFailed),
            PipelinePart.Cancelled => componentConfigurations.FirstOrDefault(x => x.ComponentId == PipelineConsts.PipelineCancelled),
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

    private static bool Verify_all_step_ids_are_unique(PipelinePartContext context)
    {
        var stepsCount = context.ComponentsConfigurations.Length;
        var uniqueStepIdsCount = context.ComponentsConfigurations.Select(x => x.StepId).Distinct().Count();
        var stepIdsAreUnique = stepsCount == uniqueStepIdsCount;
        
        if(!stepIdsAreUnique)
            context.AddError(null, PipelineValidationEntryMessage.SomeStepIdsAreNotUnique);

        return stepIdsAreUnique;
    }
}